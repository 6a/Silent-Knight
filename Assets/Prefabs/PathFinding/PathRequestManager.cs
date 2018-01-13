using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Diagnostics;

namespace PathFinding
{
    public struct PathRequest
    {
        public Transform Start { get; set; }
        public Transform End { get; set; }
        public Action<Vector2[], bool> Callback { get; set; }

        public PathRequest(Transform start, Transform end, Action<Vector2[], bool> callback)
        {
            Start = start;
            End = end;
            Callback = callback;
        }
    }

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

    public class PathRequestManager : MonoBehaviour
    {
        // I realise now that this function doesnt actually run in another thread, it just runs on the main thread.
        // When trying to make use of actual multi threading via C# ThreadPool, Multiple Unity objects break.
        // I suspect that, to convert this to true multi-threading, the whole pathfinding system would need to be rebuilt from
        // scratch without any interaction with Unity based objects. Hence MAX_THREADS being 1.
        const int MAX_THREADS = 1;

        Queue<PathResult> m_results = new Queue<PathResult>();
        Queue<PathRequest> m_requests = new Queue<PathRequest>();

        static PathRequestManager instance;

        ASPathFinder m_pathfinder;

        void Awake()
        {
            m_pathfinder = GetComponent<ASPathFinder>();
            instance = this;
        }

        void Update()
        {
            if (m_results.Count > 0)
            {
                var queueCount = m_results.Count;

                for (int i = 0; i < queueCount; i++)
                {
                    var result = m_results.Dequeue();
                    result.Callback(result.Path, result.Success);
                }
            }

            if (m_requests.Count > 0)
            {
                var queueCount = Mathf.Min(m_requests.Count, MAX_THREADS);

                for (int i = 0; i < queueCount; i++)
                {
                    var req = m_requests.Dequeue();

                    ThreadStart threadStart = delegate
                    {
                        instance.m_pathfinder.FindPath(req, instance.FinishedProcessingPath);
                    };

                    threadStart.Invoke();
                }

            }
        }

        public static void RequestPath(PathRequest request)
        {
            lock (instance.m_requests) instance.m_requests.Enqueue(request);
        }

        public void FinishedProcessingPath(PathResult result)
        {
            lock (m_results) m_results.Enqueue(result);
        }
    }
}
