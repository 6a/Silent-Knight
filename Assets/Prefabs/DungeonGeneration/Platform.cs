using UnityEngine;

namespace DungeonGeneration
{
    public class Platform
    {
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

        public bool Intersects (Platform r, int padding)
        {
            if (X - padding <= (r.X + r.Width + padding) && (X + Width + padding) >= r.X - padding
                && (Y + Height + padding) >= r.Y - padding && Y - padding <= (r.Y + r.Height + padding))
            {
                return true;
            }

            return false;
        }

        public bool IsNode()
        {
            return (Connections == 1);
        }
    }
}
