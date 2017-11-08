using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Collections;
using System;

namespace PathFinding
{
    public class ASPathFinder : MonoBehaviour
    {
        ASGrid m_grid;
        PathRequestManager m_requestManager;

        void Awake()
        {
            m_grid = GetComponent<ASGrid>();
            m_requestManager = GetComponent<PathRequestManager>();
        }

        IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            Vector2[] wayPoints = null;
            bool success = false;

            ASNode startNode = m_grid.GetNearestNode(startPos);
            ASNode targetNode = m_grid.GetNearestNode(targetPos);

            Heap<ASNode> openSet = new Heap<ASNode>(m_grid.MaxSize);
            HashSet<ASNode> closedSet = new HashSet<ASNode>();

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                var currentNode = openSet.RemoveFirst();

                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {

                    success = true;
                    break;
                }

                foreach (var neighbour in m_grid.GetNeighbours(currentNode))
                {
                    if (!neighbour.Walkable || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newMoveCost = currentNode.GCost + currentNode.GetDistance(neighbour);

                    if (newMoveCost < neighbour.GCost || !openSet.Contains(neighbour))
                    {
                        neighbour.GCost = newMoveCost;
                        neighbour.HCost = neighbour.GetDistance(targetNode);
                        neighbour.Parent = currentNode;

                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                            
                        }
                        else
                        {
                            openSet.UpdateItem(neighbour);
                        }
                    }
                }
            }

            yield return null;

            if (success)
            {
                wayPoints = RetracePath(startNode, targetNode);
            }

            if (wayPoints == null || wayPoints.Length == 0) success = false;

            stopWatch.Stop();
            UnityEngine.Debug.Log("Path found and sorted in: " + stopWatch.ElapsedMilliseconds + "ms");
            m_requestManager.FinishedProcessingPath(wayPoints, success);
        }

        public void StartFindPath(Transform start, Transform end)
        {
            StartCoroutine(FindPath(start.position, end.position));
        }

        Vector2[] RetracePath(ASNode startNode, ASNode targetNode)
        {
            var path = new List<ASNode>();
            var currentNode = targetNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;

            }

            var waypoints = path;
            waypoints.Reverse();
            return SimplifyPath(waypoints);
        }

        Vector2[] SimplifyPath(List<ASNode> path)
        {
            var waypoints = new List<Vector2>();

            for (int i = 1; i < path.Count; i++)
            {
                waypoints.Add(new Vector2(path[i].Position.x, path[i].Position.z));
            }

            waypoints = PathSmoother.douglasPeuckerReduction(waypoints, 0.5f);

            return waypoints.ToArray();
        }
    }
}