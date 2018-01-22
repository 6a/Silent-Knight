using UnityEngine;

namespace SilentKnight.DungeonGeneration
{
    /// <summary>
    /// Container representing a platform in memory.
    /// </summary>
    public class Platform
    {
        // Platform properties.
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Vector2 Center { get; set; }
        public int ID { get; private set; }
        public int Connections { get; set; }

        public Platform(int x, int y, int width, int height, Vector2 center, int id)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Center = center;
            ID = id;
            Connections = 0;
        }

        /// <summary>
        /// Returns true if this platform intersects with another. Padding increases the checked bounds of each platform.
        /// </summary>
        public bool Intersects (Platform r, int padding)
        {
            if (X - padding <= (r.X + r.Width + padding) && (X + Width + padding) >= r.X - padding
                && (Y + Height + padding) >= r.Y - padding && Y - padding <= (r.Y + r.Height + padding))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if this platform is a node (start or end platform).
        /// </summary>
        public bool IsNode()
        {
            return (Connections == 1);
        }
    }
}
