using UnityEngine;

namespace PathFinding
{
    class PathFinder : MonoBehaviour
    {
        Node[,] m_grid;

        [SerializeField] Vector2 m_gridSize;
        [SerializeField] float m_nodeRadius;
        [SerializeField] LayerMask m_walkableMask;
        

        float m_nodeDiameter;
        int m_gridX, m_gridY;
        Transform m_playerPosition = null;

        void Start()
        {
            m_nodeDiameter = m_nodeRadius * 2;
            m_gridX = Mathf.RoundToInt(m_gridSize.x / m_nodeDiameter);
            m_gridY = Mathf.RoundToInt(m_gridSize.y / m_nodeDiameter);
        }

        void Update()
        {
            if (m_playerPosition == null) m_playerPosition = FindObjectOfType<JKnightControl>().transform;
        }

        public Node GetNearestNode(Vector3 worldPos)
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
            m_grid = new Node[m_gridX, m_gridY];

            Vector3 worldBottomLeft = transform.position - (Vector3.right * (m_gridSize.x / 2)) - (Vector3.forward * (m_gridSize.y / 2));

            for (int x = 0; x < m_gridX; x++)
            {
                for (int y = 0; y < m_gridY; y++)
                {
                    Vector3 worldPos = (worldBottomLeft + Vector3.right * (x * m_nodeDiameter + m_nodeRadius) + Vector3.forward * (y * m_nodeDiameter + m_nodeRadius));

                    bool walkable = Physics.CheckSphere(worldPos, m_nodeRadius - 0.1f, m_walkableMask);

                    m_grid[x, y] = new Node(walkable, worldPos);
                }
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(m_gridSize.x, 2, m_gridSize.y));

            if (m_grid != null)
            {
                var playerNode = GetNearestNode(m_playerPosition.position);

                foreach (var node in m_grid)
                {
                    Gizmos.color = (node.Walkable) ? Color.green : Color.grey;

                    if (node == playerNode) Gizmos.color = Color.cyan;

                    Gizmos.DrawCube(node.Position + Vector3.up * 0.3f, Vector3.one * (m_nodeDiameter - 0.2f));
                }
            }
        }
    }
}
