using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Diagnostics;

namespace PathFinding
{
    /// <summary>
    /// Container for pathfinding requests.
    /// </summary>
    public struct PathRequest
    {
        // Start and destination. Transforms are used in as these will track any changes to their parent objects.
        // Vectors become inaccurate the moment that the parent objects are manipulated.
        public Transform Start { get; set; }
        public Transform End { get; set; }

        // Callback to call once pathfinding has been completed.
        public Action<Vector2[], bool> Callback { get; set; }

        public PathRequest(Transform start, Transform end, Action<Vector2[], bool> callback)
        {
            Start = start;
            End = end;
            Callback = callback;
        }
    }

    /// <summary>
    /// Represents the result of a pathfinding search, including the path itself.
    /// </summary>
    public struct PathResult
    {
        public Vector2[] Path { get; set; }
        public bool Success { get; set; }
        public Action<Vector2[], bool> Callback { get; set; }

        public PathResult(Vector2[] path, bool success, Action<Vector2[], bool> callback)
        {
            Path = path;
            Success = success;
            Callback = callback;
        }
    }

    /// <summary>
    /// Requests paths and starts new threads to process these requests.
    /// </summary>
    public class PathRequestManager : MonoBehaviour
    {
        // *I realise now that this function doesnt actually run in another thread, it just runs on the main thread.
        // When trying to make use of actual multi threading via C# ThreadPool, Multiple Unity objects break.
        // I suspect that, to convert this to true multi-threading, the whole pathfinding system would need to be rebuilt from
        // scratch without any interaction with Unity based objects. Hence MAX_THREADS being 1.
        const int MAX_THREADS = 1;

        // Queue for all calculated pathfinding results.
        Queue<PathResult> m_results = new Queue<PathResult>();

        // Queue for all pending pathfinding requests.
        Queue<PathRequest> m_requests = new Queue<PathRequest>();

        // Reference to the AStar pathfinder.
        ASPathFinder m_pathfinder;

        static PathRequestManager instance;

        void Awake()
        {
            m_pathfinder = GetComponent<ASPathFinder>();
            instance = this;
        }

        void Update()
        {
            // If there are any results pin the queue, remove them from the queue and call the
            // appropriate callback function.
            if (m_results.Count > 0)
            {
                var queueCount = m_results.Count;

                for (int i = 0; i < queueCount; i++)
                {
                    var result = m_results.Dequeue();
                    result.Callback(result.Path, result.Success);
                }
            }

            // If there are any requests in the queue, start a new thread* and initialise the path request.
            // *The number of threads is limited to one.
            if (m_requests.Count > 0)
            {
                var queueCount = Mathf.Min(m_requests.Count, MAX_THREADS);

                for (int i = 0; i < queueCount; i++)
                {
                    var req = m_requests.Dequeue();

                    // Starts a new thread and runs the pathfinding request on it.
                    ThreadStart threadStart = delegate
                    {
                        instance.m_pathfinder.FindPath(req, instance.FinishedProcessingPath);
                    };

                    threadStart.Invoke();
                }

            }
        }

        /// <summary>
        /// Request a new pathfinding operation.
        /// </summary>
        public static void RequestPath(PathRequest request)
        {
            lock (instance.m_requests) instance.m_requests.Enqueue(request);
        }

        /// <summary>
        /// Adds the a pathfinding result to the internal queue.
        /// </summary>
        void FinishedProcessingPath(PathResult result)
        {
            lock (m_results) m_results.Enqueue(result);
        }
    }
}
