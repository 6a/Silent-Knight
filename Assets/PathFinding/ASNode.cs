using System;
using UnityEngine;

namespace PathFinding
{
    public class ASNode : IHeapable<ASNode>
    {
        public bool Walkable { get; set; }
        public Vector3 Position { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public int GCost { get; set; }
        public int HCost { get; set; }
        public int FCost { get { return GCost + HCost; } }
        public ASNode Parent { get; set; }

        public int HeapIndex { get; set; }

        public int RegisterIndex { get; set; }

        const int DIAGCOST = 14;
        const int LINEARCOST = 10;

        public ASNode(bool walkable, Vector3 position, int x, int y, int registerIndex)
        {
            Walkable = walkable;
            Position = position;
            X = x;
            Y = y;
            RegisterIndex = registerIndex;
        }

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

        public int CompareTo(ASNode comparisonNode)
        {
            var comparison = FCost.CompareTo(comparisonNode.FCost);

            if (comparison == 0)
            {
                comparison = HCost.CompareTo(comparisonNode.HCost);
            }

            return -comparison;
        }
    }
}
