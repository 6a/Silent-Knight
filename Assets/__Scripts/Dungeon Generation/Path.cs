using UnityEngine;

namespace SilentKnight.DungeonGeneration
{
    /// <summary>
    /// Container for storing data representing a path between two platforms.
    /// </summary>
    public struct Path
    {
        // The start of the path.
        public Vector2 Origin { get; set; }

        // The start of the branch (for non-linear paths).
        public Vector2 Branch { get; set; }

        // The direction of the path starting tile.
        public Vector2 StartVector { get; set; }

        // The direction of the path branch node (for non-linear paths).
        public Vector2 BranchVector { get; set; }

        // Represents the state of this path, true if linear.
        public bool IsStraight { get; set; }

        /// <summary>
        /// Constructor for linear paths.
        /// </summary>
        public Path(Vector2 start, Vector2 end)
        {
            Origin = start;
            Branch = end;
            StartVector = Branch - Origin;
            BranchVector = Vector2.zero;
            IsStraight = true;
        }

        /// <summary>
        /// Constructor for non-linear paths.
        /// </summary>
        public Path(Vector2 start, Vector2 branch, Vector2 startVector, Vector2 branchVector)
        {
            Origin = start;
            Branch = branch;
            StartVector = startVector;
            BranchVector = branchVector;
            IsStraight = false;
        }
    }
}