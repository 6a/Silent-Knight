using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace PathFinding
{
    public class PathRequestManager : MonoBehaviour
    {
        struct PathRequest
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

        Queue<PathRequest> m_pathRequestQueue;
        PathRequest m_currentPathRequest;
        static PathRequestManager instance;

        ASPathFinder m_pathfinder;

        bool m_busy;

        void Awake()
        {
            m_pathRequestQueue = new Queue<PathRequest>();
            m_pathfinder = GetComponent<ASPathFinder>();
            instance = this;
        }

        public static void RequestPath(Transform pathStart, Transform pathEnd, Action<Vector2[], bool> callback)
        {
            var newReq = new PathRequest(pathStart, pathEnd, callback);
            instance.m_pathRequestQueue.Enqueue(newReq);
            instance.TryProcessNext();
        }

        public void FinishedProcessingPath(Vector2[] path, bool success)
        {
            m_currentPathRequest.Callback(path, success);
            m_busy = false;
            TryProcessNext();
        }

        void TryProcessNext()
        {
            if (!m_busy && m_pathRequestQueue.Count > 0)
            {
                m_currentPathRequest = m_pathRequestQueue.Dequeue();
                m_busy = true;
                m_pathfinder.StartFindPath(m_currentPathRequest.Start, m_currentPathRequest.End);
            }
        }
    }
}
