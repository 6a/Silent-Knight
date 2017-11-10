using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;


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
        Queue<PathResult> m_results = new Queue<PathResult>();

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

                lock (m_results)
                {
                    print(queueCount);
                    for (int i = 0; i < queueCount; i++)
                    {
                        var result = m_results.Dequeue();
                        result.Callback(result.Path, result.Success);
                    }
                }
            }
        }

        public static void RequestPath(PathRequest request)
        {
            ThreadStart threadStart = delegate
            {
                instance.m_pathfinder.FindPath(request, instance.FinishedProcessingPath);
            };

            threadStart.Invoke();
        }

        public void FinishedProcessingPath(PathResult result)
        {
            lock (m_results) m_results.Enqueue(result);
        }
    }
}
