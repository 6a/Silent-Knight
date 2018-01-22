using System.Collections.Generic;
using UnityEngine;

namespace SilentKnight.PathFinding
{
    /// <summary>
    /// Generates and handles the Astart navmesh.
    /// </summary>
    class ASGrid : MonoBehaviour
    {
        // The actual A* Grid in memory, as a 2D array of nodes.
        ASNode[,] m_grid;

        // Size of the grid (equal in both axis).
        [SerializeField] Vector2 m_gridSize;

        // Radius of the search node (changes grid resolution).
        [SerializeField] float m_nodeRadius;

        // The layer that is walkable.
        [SerializeField] LayerMask m_walkableMask;

        // Max width/height this grid will represent.
        public int MaxSize { get { return m_grid.Length; } }

        // Grid properties.
        float m_nodeDiameter;
        int m_gridX, m_gridY;

        static ASGrid instance;

        void Awake()
        {
            m_nodeDiameter = m_nodeRadius * 2;
            m_gridX = Mathf.RoundToInt(m_gridSize.x / m_nodeDiameter);
            m_gridY = Mathf.RoundToInt(m_gridSize.y / m_nodeDiameter);
            instance = this;
        }

        /// <summary>
        /// Returns true if the Vector3 position passed is not within the walkable area.
        /// </summary>
        public static bool IsOffGrid(Vector3 pos)
        {
            var n = GetNearestNode(pos);

            if (n.OutOfBounds) return true;
            return false;
        }

        /// <summary>
        /// Updates the current grid. Unblocks previousPos, and blocks currentPos.
        /// </summary>
        public static void UpdateGrid(Vector3 previousPos, Vector3 currentPos)
        {
            var previousIndices = instance.GetNearestNodeIndeces(previousPos);
            var currentIndices = instance.GetNearestNodeIndeces(currentPos);

            instance.m_grid[(int)currentIndices.x, (int)currentIndices.y].Blocked = true;

            if (currentIndices != previousIndices)
            {
                var node = instance.m_grid[(int)previousIndices.x, (int)previousIndices.y];
                node.Blocked = false;
            }
        }

        /// <summary>
        /// Returns a list of all the valid neighbours surrounding a particular node (tile).
        /// </summary>
        public List<ASNode> GetNeighbours(ASNode refNode)
        {
            List<ASNode> neighbours = new List<ASNode>();

            // Checks every single position around the refnode position, and ensures it is a valid location within the grid.
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

        /// <summary>
        /// Returns a reference to the closest, valid node to the postion provided.
        /// </summary>
        public static ASNode GetNearestValidNode(Vector3 pos)
        {
            var neighbours = instance.GetNeighbours(GetNearestNode(pos));

            // Checks each neighbour for validity (walkable). If no walkable tiles are found, there are none nearby and
            // null is returned.
            foreach (var n in neighbours)
            {
                if (n.Walkable) return n;
            }

            return null;
        }

        /// <summary>
        /// Returns a reference to the node nearest (centrally) to the world position provided.
        /// </summary>
        public static ASNode GetNearestNode(Vector3 worldPos)
        {
            float percentX = (worldPos.x + instance.m_gridSize.x / 2.0f) / instance.m_gridSize.x;
            float percentY = (worldPos.z + instance.m_gridSize.y / 2.0f) / instance.m_gridSize.y;

            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int x = Mathf.Clamp((int)((instance.m_gridX) * percentX), 0, instance.m_gridX - 1);
            int y = Mathf.Clamp((int)((instance.m_gridY) * percentY), 0, instance.m_gridY - 1);

            var n = instance.m_grid[x, y];

            return n;
        }

        /// <summary>
        /// Returns a Vector2 representing the position of the nearest node on the grid.
        /// </summary>
        public Vector2 GetNearestNodeIndeces(Vector3 worldPos)
        {
            float percentX = (worldPos.x + m_gridSize.x / 2.0f) / m_gridSize.x;
            float percentY = (worldPos.z + m_gridSize.y / 2.0f) / m_gridSize.y;

            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int x = Mathf.Clamp((int)((m_gridX) * percentX), 0, m_gridX - 1);
            int y = Mathf.Clamp((int)((m_gridY) * percentY), 0, m_gridY - 1);
            return new Vector2(x, y);
        }

        /// <summary>
        /// Creates a grid for the current dungeon layout.
        /// </summary>
        public void CreateGrid()
        {
            // Create a new blank grid.
            m_grid = new ASNode[m_gridX, m_gridY];

            // Locate the bottom left position in world coordinates.
            Vector3 worldBottomLeft = transform.position - (Vector3.right * (m_gridSize.x / 2)) - (Vector3.forward * (m_gridSize.y / 2));

            int nodeNumber = 0;

            // For each tile on the grid, determine its state as a node.
            // Uses Physics.Checksphere to physically probe the current location.
            for (int x = 0; x < m_gridX; x++)
            {
                for (int y = 0; y < m_gridY; y++)
                {
                    Vector3 worldPos = (worldBottomLeft + Vector3.right * (x * m_nodeDiameter + m_nodeRadius) + Vector3.forward * (y * m_nodeDiameter + m_nodeRadius));

                    bool walkable = Physics.CheckSphere(worldPos, m_nodeRadius - 0.1f, m_walkableMask);

                    m_grid[x, y] = new ASNode(walkable, worldPos, x, y, nodeNumber);

                    if (!walkable) m_grid[x, y].OutOfBounds = true;

                    nodeNumber++;
                }
            }

            // Create a new list of nodes.
            List<ASNode> invalidNodes = new List<ASNode>();

            // Identify all invalid nodes. Any node touching the air is considered invalid, to provide some padding to the grid.
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

            // Update all the nodes that are found to be invalid and tag them as out of bounds.
            foreach (var invalidNode in invalidNodes)
            {
                invalidNode.Walkable = false;
                invalidNode.OutOfBounds = true;
            }
        }

        /// <summary>
        /// Handles drawing of the grid boundaries and grid tiles.
        /// </summary>
        void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(m_gridSize.x, 2, m_gridSize.y));

            if (m_grid != null)
            {
                foreach (var node in m_grid)
                {
                    if (!node.Walkable) continue;
                    if (node.Blocked) Gizmos.color = Color.blue;
                    else Gizmos.color = Color.green;

                    Gizmos.DrawCube(node.Position + Vector3.up * 0.8f, Vector3.one * m_nodeRadius * 1.8f);
                }
            }
        }
    }
}
