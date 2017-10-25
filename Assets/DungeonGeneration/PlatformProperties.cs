namespace DungeonGeneration
{
    public struct PlatformProperties
    {
        public int MinX { get; set; }
        public int MaxX { get; set; }
        public int MinY { get; set; }
        public int MaxY { get; set; }

        public PlatformProperties(int minx, int maxx, int miny, int maxy)
        {
            MinX = minx;
            MaxX = maxx;
            MinY = miny;
            MaxY = maxy;
        }
    }
}
