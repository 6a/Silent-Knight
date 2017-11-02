using UnityEngine;

namespace PathFinding
{
    class Node
    {
        public bool Walkable { get; set; }
        public Vector3 Position { get; set; }

        public Node(bool walkable, Vector3 position)
        {
            Walkable = walkable;
            Position = position;
        }
    }
}
