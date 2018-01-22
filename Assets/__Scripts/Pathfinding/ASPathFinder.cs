using System.Collections.Generic;
using UnityEngine;
using System;
using SilentKnight.Utility;

namespace SilentKnight.PathFinding
{
    /// <summary>
    /// Handles calculation of A* paths.
    /// </summary>
    public class ASPathFinder : MonoBehaviour
    {
        // Reference to grid object.
        ASGrid m_grid;

        void Awake()
        {
            m_grid = GetComponent<ASGrid>();
        }

        /// <summary>
        /// Finds the shortest path and calls the callback function when completed.
        /// </summary>
        public void FindPath(PathRequest request, Action<PathResult> callback)
        {
            // Create a blank aray of waypoints.
            Vector2[] wayPoints = null;
            bool success = false;

            // Locate start and end node.
            ASNode startNode = ASGrid.GetNearestNode(request.Start.position);
            ASNode targetNode = ASGrid.GetNearestNode(request.End.position);

            // Create a heap that stores the nodes that are currently being checked.
            Heap<ASNode> openSet = new Heap<ASNode>(m_grid.MaxSize);

            // Create a hashset containing all the discarded nodes.
            HashSet<ASNode> closedSet = new HashSet<ASNode>();

            // Add the startnode to the open set to begin the A* search.
            openSet.Add(startNode);

            ASNode currentNode = null;

            // While there are still nodes in the openSet, and a valid path has not been found, search for one.
            while (openSet.Count > 0)
            {

                // Get a node from the openSet.
                currentNode = openSet.RemoveFirst();

                // Add a reference to the current node that was just found to the closed set to prevent it from 
                // being checked again afterwards.
                closedSet.Add(currentNode);

                // If the node found is the targetnode, the best path must have been found, so the loop exits.
                if (currentNode == targetNode)
                {
                    success = true;
                    break;
                }

                // Find all node neighbours and check them for validity.
                foreach (var neighbour in m_grid.GetNeighbours(currentNode))
                {
                    // If the neighbour is not walkable or has been discarded already, ignore it.
                    if (!neighbour.Walkable || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    // Calculate the cost of the potential move.
                    int newMoveCost = currentNode.GCost + currentNode.GetDistance(neighbour) + currentNode.MovementPenalty;

                    // Update the neighbours values appropriately, and add it to the openSet. If it is already in the
                    // openset, it must have changed for some reason (such as exploring other neighbours), so update
                    // it instead.
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

            // If success is true, a path was found. Retrace the path to flip, so that it is ordered correctly.
            if (success)
            {
                wayPoints = RetracePath(startNode, targetNode);
            }

            // If something went wrong, success is set back to null so that the callback function acts appropriately.
            if (wayPoints == null || wayPoints.Length == 0) success = false;

            // call the callback function with the results of the pathfinding search.
            callback(new PathResult(wayPoints, success, request.Callback));
        }

        /// <summary>
        /// Returns a list representing the path, correctly re-verted to go from start to finish.
        /// </summary>
        Vector2[] RetracePath(ASNode startNode, ASNode targetNode)
        {
            // The path is traced backwards starting at the targetNode, by checking
            // the node references, similar to a linked list operation.

            var path = new List<ASNode>();
            var currentNode = targetNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            }

            // The list is then reversed to be in the correct order.
            var waypoints = path;
            waypoints.Reverse();
            
            // Returns a simplified version of the path, which irons out kinks, and simplifies lines.
            return SimplifyPath(waypoints);
        }

        /// <summary>
        /// Returns an array representing a simplified version of a path.
        /// </summary>
        /// <param name="path"></param>
        Vector2[] SimplifyPath(List<ASNode> path)
        {
            // Uses Douglas Peucker reduction to simplify a path, taking "a curve composed of line
            // segments and finds a similar curve with fewer points".

            var waypoints = new List<Vector2>();

            for (int i = 0; i < path.Count; i++)
            {
                waypoints.Add(new Vector2(path[i].Position.x, path[i].Position.z));
            }
            if (waypoints.Count > 1) waypoints = PathSmoother.Reduce(waypoints, 0.5f);

            return waypoints.ToArray();
        }
    }
}