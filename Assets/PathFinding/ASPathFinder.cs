using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Diagnostics;

namespace PathFinding
{
    public class ASPathFinder : MonoBehaviour
    {
        ASGrid m_grid;

        Dictionary<int, ASPath> m_paths = new Dictionary<int, ASPath>();
        int m_nextID;

        void Awake()
        {
            m_grid = GetComponent<ASGrid>();
            m_nextID = 0;
        }

        void Update()
        {
        }

        public List<ASNode> GetPath(int id)
        {
            FindPath(m_paths[id].Seeker.position, m_paths[id].Target.position, id);
            return m_grid.GetPath(id);
        }

        public void UpdatePathTarget(int id, Transform newTarget)
        {
            m_paths[id] = new ASPath(m_paths[id].Seeker, newTarget);
        }

        public int Register(Transform seeker, Transform target)
        {
            var path = new ASPath(seeker, target);

            if (!m_paths.Any(i => i.Value.Equals(path)))
            {
                m_paths.Add(m_nextID, path);
                m_nextID++;
                return m_nextID - 1;
            }

            UnityEngine.Debug.Log("This set of transforms is already registered");
            return -1;
        }
        
        public void UnRegister(int id)
        {
            m_paths.Remove(id);
        }

        void FindPath(Vector3 startPos, Vector3 targetPos, int pathID)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

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
                    stopWatch.Stop();
                    UnityEngine.Debug.Log("Path " + pathID + " found in: " + stopWatch.ElapsedMilliseconds + "ms");
                    RetracePath(startNode, targetNode, pathID);
                    return;
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
        }

        void RetracePath(ASNode startNode, ASNode targetNode, int id)
        {
            var path = new List<ASNode>();
            var currentNode = targetNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            }

            path.Reverse();

            m_grid.RegisterPath(id, path);
        }
    }
}