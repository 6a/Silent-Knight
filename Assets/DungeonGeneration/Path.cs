using UnityEngine;

namespace DungeonGeneration
{
    public struct Path
    {
        public Vector2 Origin1 { get; set; }
        public Vector2 Path1 { get; set; }
        public Vector2 Origin2 { get; set; }
        public Vector2 Path2 { get; set; }

        public Path(Vector2 origin1, Vector2 path1, Vector2 origin2, Vector2 path2)
        {
            Origin1 = origin1;
            Path1 = path1;
            Origin2 = origin2;
            Path2 = path2;
        }
    }

}