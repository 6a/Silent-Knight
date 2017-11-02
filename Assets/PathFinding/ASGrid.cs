using System.Collections.Generic;
using UnityEngine;

namespace PathFinding
{
    class ASGrid : MonoBehaviour
    {
        ASNode[,] m_grid;

        [SerializeField] Vector2 m_gridSize;
        [SerializeField] float m_nodeRadius;
        [SerializeField] LayerMask m_walkableMask;

        public Dictionary<int, List<ASNode>> Paths { get; private set; }

        public int MaxSize { get { return m_grid.Length; } }

        float m_nodeDiameter;
        int m_gridX, m_gridY;

        void Awake()
        {
            Paths = new Dictionary<int, List<ASNode>>();
        }

        void Start()
        {
            m_nodeDiameter = m_nodeRadius * 2;
            m_gridX = Mathf.RoundToInt(m_gridSize.x / m_nodeDiameter);
            m_gridY = Mathf.RoundToInt(m_gridSize.y / m_nodeDiameter);
        }

        public void RegisterPath (int id, List<ASNode> path)
        {
            Paths[id] = path;
        }

        public List<ASNode> GetNeighbours(ASNode refNode)
        {
            List<ASNode> neighbours = new List<ASNode>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }

                    var checkX = refNode.X + x;
                    var checkY = refNode.Y + y;

                    if (checkX >= 0 && checkX < m_gridX && checkY >= 0 & checkY < m_gridY)
                    {
                        neighbours.Add(m_grid[checkX, checkY]);
                    }
                }
            }

            return neighbours;
        }

        public ASNode GetNearestNode(Vector3 worldPos)
        {
            float percentX = (worldPos.x + m_gridSize.x / 2) / m_gridSize.x;
            float percentY = (worldPos.z + m_gridSize.y / 2) / m_gridSize.y;

            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int x = Mathf.RoundToInt((m_gridX - 1) * percentX);
            int y = Mathf.RoundToInt((m_gridY - 1) * percentY);

            return m_grid[x, y];
        }

        public void CreateGrid()
        {
            m_grid = new ASNode[m_gridX, m_gridY];

            Vector3 worldBottomLeft = transform.position - (Vector3.right * (m_gridSize.x / 2)) - (Vector3.forward * (m_gridSize.y / 2));

            int nodeNumber = 0;

            for (int x = 0; x < m_gridX; x++)
            {
                for (int y = 0; y < m_gridY; y++)
                {
                    Vector3 worldPos = (worldBottomLeft + Vector3.right * (x * m_nodeDiameter + m_nodeRadius) + Vector3.forward * (y * m_nodeDiameter + m_nodeRadius));

                    bool walkable = Physics.CheckSphere(worldPos, m_nodeRadius - 0.1f, m_walkableMask);

                    m_grid[x, y] = new ASNode(walkable, worldPos, x, y, nodeNumber);
                    nodeNumber++;
                }
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(m_gridSize.x, 2, m_gridSize.y));

            if (m_grid != null)
            {
                foreach (var node in m_grid)
                {
                    Gizmos.color = (node.Walkable) ? Color.green : Color.grey;

                    if (Paths != null)
                    {
                        if (Paths[0].Contains(node))
                        {
                            Gizmos.color = Color.black;
                        }
                    }

                    Gizmos.DrawCube(node.Position + Vector3.up * 0.3f, Vector3.one * (m_nodeDiameter - 0.2f));
                }
            }
        }
    }
}
