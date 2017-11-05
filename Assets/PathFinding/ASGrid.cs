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

        public int MaxSize { get { return m_grid.Length; } }

        float m_nodeDiameter;
        int m_gridX, m_gridY;

        void Awake()
        {
            m_nodeDiameter = m_nodeRadius * 2;
            m_gridX = Mathf.RoundToInt(m_gridSize.x / m_nodeDiameter);
            m_gridY = Mathf.RoundToInt(m_gridSize.y / m_nodeDiameter);
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
            float percentX = (worldPos.x + m_gridSize.x / 2.0f) / m_gridSize.x;
            float percentY = (worldPos.z + m_gridSize.y / 2.0f) / m_gridSize.y;

            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int x = Mathf.Clamp((int)((m_gridX) * percentX), 0, m_gridX - 1);
            int y = Mathf.Clamp((int)((m_gridY) * percentY), 0, m_gridY - 1);
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

            List<ASNode> invalidNodes = new List<ASNode>();

            for (int x = 0; x < m_gridX; x++)
            {
                for (int y = 0; y < m_gridY; y++)
                {
                    var neighbours = GetNeighbours(m_grid[x, y]);

                    foreach (var neighbour in neighbours)
                    {
                        if (!neighbour.Walkable)
                        {
                            invalidNodes.Add(m_grid[x, y]);
                        }
                    }
                }
            }

            foreach (var invalidNode in invalidNodes)
            {
                invalidNode.Walkable = false;
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(m_gridSize.x, 2, m_gridSize.y));

            if (m_grid != null)
            {
                foreach (var node in m_grid)
                {
                    if (!node.Walkable) continue;
                    Gizmos.color = Color.green;

                    Gizmos.DrawCube(node.Position + Vector3.up * 0.8f, Vector3.one * m_nodeRadius * 1.8f);
                }
            }
        }
    }
}
