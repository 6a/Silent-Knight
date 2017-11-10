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

        void Awake()
        {
            m_grid = GetComponent<ASGrid>();
        }

        public void FindPath(PathRequest request, Action<PathResult> callback)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            Vector2[] wayPoints = null;
            bool success = false;

            ASNode startNode = m_grid.GetNearestNode(request.Start.position);
            ASNode targetNode = m_grid.GetNearestNode(request.End.position);

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

            if (success)
            {
                wayPoints = RetracePath(startNode, targetNode);
            }

            if (wayPoints == null || wayPoints.Length == 0) success = false;

            stopWatch.Stop();
            UnityEngine.Debug.Log("Path found and sorted in: " + stopWatch.ElapsedMilliseconds + "ms");
            callback(new PathResult(wayPoints, success, request.Callback));
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