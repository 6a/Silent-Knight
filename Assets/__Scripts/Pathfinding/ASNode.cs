using UnityEngine;
using SilentKnight.Utility;

namespace SilentKnight.PathFinding
{
    /// <summary>
    /// Represents an A* pathfinding node.
    /// </summary>
    public class ASNode : IHeapable<ASNode>
    {
        // Various node properties.
        public bool Walkable { get; set; }
        public bool OutOfBounds { get; set; }
        public bool Blocked { get; set; }
        public int MovementPenalty { get { return (Blocked) ? 10 : 0; } }

        // Positional data
        public Vector3 Position { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        // Distance from starting node.
        public int GCost { get; set; }

        // Distance from end node (heuristic).
        public int HCost { get; set; }

        // Total cost.
        public int FCost { get { return GCost + HCost; } }

        // Parent node in the chain.
        public ASNode Parent { get; set; }

        // Index within the heap.
        public int HeapIndex { get; set; }

        // Nodes reference number.
        public int RegisterIndex { get; set; }

        // Constant values for calculating costs.
        const int DIAGCOST = 14;
        const int LINEARCOST = 10;

        public ASNode(bool walkable, Vector3 position, int x, int y, int registerIndex)
        {
            Walkable = walkable;
            Position = position;
            X = x;
            Y = y;
            RegisterIndex = registerIndex;

            OutOfBounds = false;
            Blocked = false;
        }

        /// <summary>
        /// Gets the distance between two nodes, by moving in line with the grid.
        /// </summary>
        /// SOURCE: https://imgur.com/z4Scazg
        public int GetDistance(ASNode target)
        {
            int xDistance = Mathf.Abs(target.X - X);
            int yDistance = Mathf.Abs(target.Y - Y);

            if (xDistance >= yDistance)
            {
                return (DIAGCOST * yDistance) + (LINEARCOST * (xDistance - yDistance));
            }
            else
            {
                return (DIAGCOST * xDistance) + (LINEARCOST * (yDistance - xDistance));
            }
        }

        /// <summary>
        /// Returns 1 if the current item has a higher priority to the comparator.
        /// </summary>
        public int CompareTo(ASNode comparisonNode)
        {
            // Compares FCost, and if they are equal, also compares HCost.

            var comparison = FCost.CompareTo(comparisonNode.FCost);

            if (comparison == 0)
            {
                comparison = HCost.CompareTo(comparisonNode.HCost);
            }

            // return negative as this function is technically the reverse of the standard CompareTo function.
            return -comparison;
        }
    }
}
