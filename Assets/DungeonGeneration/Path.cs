using UnityEngine;

namespace DungeonGeneration
{
    public struct Path
    {
        public Vector2 Origin { get; set; }
        public Vector2 Branch { get; set; }

        public Vector2 StartVector { get; set; }
        public Vector2 BranchVector { get; set; }

        public bool IsStraight { get; set; }

        public Path(Vector2 start, Vector2 end)
        {
            Origin = start;
            Branch = end;
            StartVector = Branch - Origin;
            BranchVector = Vector2.zero;
            IsStraight = true;
        }

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