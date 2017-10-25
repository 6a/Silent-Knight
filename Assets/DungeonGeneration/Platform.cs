namespace DungeonGeneration
{
    class Platform
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Platform(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
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
    }
}
