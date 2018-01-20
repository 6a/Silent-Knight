using UnityEngine;
using System.Collections.Generic;
using Delaunay;

namespace DungeonGeneration
{
    /// <summary>
    /// Performs dungeon generation and fabrication.
    /// </summary>
    public static class Generator
    {
        // Properties of the dungeon that will be generated.
        static PlatformProperties m_platformProperties;
        static int m_width, m_height;
        static int m_cycles;
        static int m_seed;
        static int m_padding;
        static int m_minPlatforms;
        static char m_emptyChar;
        static char m_platformChar;
        static char m_nodeChar;
        static char m_pathChar;
        static int m_scale;
        static int m_offset;
        static int m_level;

        // Current dungeon in memory.
        public static Dungeon CurrentDungeon { get; set; }

        public static void Init(int width, int height, PlatformProperties platformProperties, 
        int cycles, int padding, int minPlatforms, char emptyChar, char platformChar, char nodeChar,
        char pathChar, int scale, int offset, int level)
        {
            m_width = width;
            m_height = height;
            m_platformProperties = platformProperties;
            m_cycles = cycles;
            m_padding = padding;
            m_minPlatforms = minPlatforms;
            m_emptyChar = emptyChar;
            m_platformChar = platformChar;
            m_nodeChar = nodeChar;
            m_pathChar = pathChar;
            m_scale = scale;
            m_offset = offset;
            m_level = level;
        }

        /// <summary>
        /// Initialise the loading of a new level.
        /// </summary>
        public static void InitNewLevel(int level)
        {
            m_level = level;
        }

        /// <summary>
        /// Creates a fabricator and constructs the dungeon stored in memory.
        /// </summary>
        public static void Fabricate()
        {
            var fabricator = new Fabricator(CurrentDungeon, m_scale, m_offset, m_level);

            fabricator.Fabricate();

            fabricator.PlaceUnits();
        }

        /// <summary>
        /// Generates a new dungeon in memory using a seed (defaults to 0).
        /// </summary>
        public static void GenerateNewDungeon(int seed = 0)
        {
            m_seed = seed;
            Random.InitState(m_seed);
            Generate();
        }

        /// <summary>
        /// Fabricates the current dungeon by index, as test (skips a few non-critical steps such as registering the 
        /// player unit with the camera.
        /// </summary>
        public static void FabricateTest(int level)
        {
            var fabricator = new Fabricator(CurrentDungeon, m_scale, m_offset, level);
            fabricator.FabricateTest();
        }

        /// <summary>
        /// Handles the actual generation of a new dungeon.
        /// </summary>
        static void Generate ()
        {
            // Create new blank containers in memory.
            List<Platform> platforms = new List<Platform>();
            var platformBounds = new List<PlatformBounds>();
            List<Path> paths = new List<Path>();

            // For each cycle to execute, create a random platform in memory and determines whether its placement
            // is valid. Invalid placements are either out of bounds, or overlap existing platforms.
            for (int i = 0; i < m_cycles; i++)
            {
                var platform = GeneratePlatform(i);

                bool isValid = true;

                foreach (var p in platforms)
                {
                    if (p.Intersects(platform, m_padding))
                    {
                        isValid = false;
                    }
                }

                if (isValid)
                {
                    platforms.Add(platform);
                    platformBounds.Add(new PlatformBounds(new Vector2(platform.X * 2 - 32, platform.Y * 2 - 32), new Vector2((platform.X * 2 - 32) + platform.Width * 2, (platform.Y * 2 - 32) + platform.Height * 2)));
                }
            }

            // Updates the platform class instance with the new platform data.
            Platforms.Update(platformBounds);

            // Sets up a List of points representing the centers of each platform.
            List<Vector2> centers = new List<Vector2>();
            List<uint> colors = new List<uint>();
            foreach (var platform in platforms)
            {
                colors.Add(0);
                centers.Add(platform.Center);
            }

            // Calculate the minimum spanning tree for the platform list.
            Voronoi voronoi = new Voronoi(centers, colors, new Rect(0, 0, m_width, m_height));
            var minSpanningTree = voronoi.SpanningTree(KruskalType.MINIMUM);

            // Create paths between the platforms. Each line is a connection between two centers.
            foreach (var line in minSpanningTree)
            {
                // Identify the first platform, by taking the line's first point and searching for a match
                // within the platform database.
                var platform0 = platforms.Find(i => i.Center == line.p0.Value);

                // Identify the second platform, by taking the line's second point and searching for a match
                // within the platform database.
                var platform1 = platforms.Find(i => i.Center == line.p1.Value);

                // Increment number of connections for each platform.
                platform1.Connections++;
                platform0.Connections++;

                // Find the aligned direction for the platform.
                var dir = (new Vector2(platform1.X, platform1.Y) - new Vector2(platform0.X, platform0.Y)).Align2D();

                // Based on the direction of the path, forge a path.
                // Behavious changes depending on whether a straight path can be made or not.
                // Straight path: Forge a path in the facing direction up to the edge of the adjacent platform.
                // Non-straight path: Forge a path in the facing direction up to the top of the path is adjacent to the target platform.
                if (dir == Vector2.up)
                {
                    if (IsWithinBounds(platform0, platform1, true))
                    {
                        var minx = (platform0.X >= platform1.X) ? platform0.X : platform1.X;
                        var maxx = (platform0.X + platform0.Width <= platform1.X + platform1.Width) ? platform0.X + platform0.Width : platform1.X + platform1.Width;
                        var randX = Random.Range(minx, maxx);

                        var pathStart = new Vector2(randX, platform0.Y + platform0.Height);

                        var pathEnd = new Vector2(randX, platform1.Y);

                        paths.Add(new Path(pathStart, pathEnd));
                    }
                    else
                    {
                        Vector2 pathStart, branchStart;
                        Vector2 startVec, branchVec;

                        if (platform0.X > platform1.X)
                        {
                            pathStart = new Vector2(platform0.X, platform0.Y + platform0.Height);
                            branchStart = new Vector2(platform0.X, platform1.Y);
                            branchVec = new Vector2(platform1.X + platform1.Width - 1 - branchStart.x, 0); 
                        }
                        else
                        {
                            pathStart = new Vector2(platform0.X + platform0.Width - 1, platform0.Y + platform0.Height);
                            branchStart = new Vector2(platform0.X + platform0.Width - 1, platform1.Y);
                            branchVec = new Vector2(platform1.X - branchStart.x, 0);
                        }


                        startVec = new Vector2(0, branchStart.y - pathStart.y);

                        paths.Add(new Path(pathStart, branchStart, startVec, branchVec));
                    }
                }
                else if (dir == Vector2.down)
                {
                    // There is never a case where the path is downwards, probably due to the way that the Delauney library organises it's points internally.
                }
                else if (dir == Vector2.left)
                {
                    if (IsWithinBounds(platform0, platform1, false))
                    {
                        var miny = (platform0.Y >= platform1.Y) ? platform0.Y : platform1.Y;
                        var maxy = (platform0.Y + platform0.Height <= platform1.Y + platform1.Height) ? platform0.Y + platform0.Height : platform1.Y + platform1.Height;
                        var randY = Random.Range(miny, maxy);

                        var pathStart = new Vector2(platform0.X - 1, randY);

                        var pathEnd = new Vector2(platform1.X + platform1.Width - 1, randY);

                        paths.Add(new Path(pathStart, pathEnd));
                    }
                    else
                    {
                        Vector2 pathStart, branchStart;
                        Vector2 startVec, branchVec;

                        pathStart = new Vector2(platform0.X - 1, platform0.Y + platform0.Height - 1);
                        branchStart = new Vector2(platform1.X + platform1.Width - 1, platform0.Y + platform0.Height);
                        branchVec = new Vector2(0, platform1.Y - (platform0.Y + platform0.Height));

                        startVec = new Vector2(branchStart.x - pathStart.x - 1, 0);

                        paths.Add(new Path(pathStart, branchStart, startVec, branchVec));
                    }

                }
                else if (dir == Vector2.right)
                {
                    if (IsWithinBounds(platform0, platform1, false))
                    {
                        var miny = (platform0.Y >= platform1.Y) ? platform0.Y : platform1.Y;
                        var maxy = (platform0.Y + platform0.Height <= platform1.Y + platform1.Height) ? platform0.Y + platform0.Height : platform1.Y + platform1.Height;
                        var randY = Random.Range(miny, maxy);

                        var pathStart = new Vector2(platform0.X + platform0.Width, randY);

                        var pathEnd = new Vector2(platform1.X, randY);

                        paths.Add(new Path(pathStart, pathEnd));
                    }
                    else
                    {
                        Vector2 pathStart, branchStart;
                        Vector2 startVec, branchVec;

                        pathStart = new Vector2(platform0.X + platform0.Width, platform0.Y + platform0.Height - 1);
                        branchStart = new Vector2(platform1.X, platform0.Y + platform0.Height - 1);
                        branchVec = new Vector2(0, platform1.Y - (platform0.Y + platform0.Height - 1));

                        startVec = new Vector2(branchStart.x - pathStart.x, 0);

                        paths.Add(new Path(pathStart, branchStart, startVec, branchVec));
                    }
                }
            }

            // Add all the platform/path data to a new dungeon object.
            CurrentDungeon = new Dungeon(platforms, paths, m_width, m_height, m_emptyChar, m_platformChar, m_nodeChar, m_pathChar);
        }

        /// <summary>
        /// Generates a platform with a specific ID number, within the bounds of current dungeon properties.
        /// </summary>
        static Platform GeneratePlatform(int id)
        {
            int w, h, x, y;
            Vector2 center;

            w = Random.Range(m_platformProperties.MinX, m_platformProperties.MaxX);
            h = Random.Range(m_platformProperties.MinY, m_platformProperties.MaxY);
            x = Random.Range(0, m_width + 1 - w);
            y = Random.Range(0, m_height + 1 - h);
            center = new Vector2(x + (w / 2), y + (h / 2));
            var platform = new Platform(x, y, w, h, center, id);

            return platform;
        }

        /// <summary>
        /// Checks whether two platforms are overlapping.
        /// </summary>
        static bool IsWithinBounds(Platform platform0, Platform platform1, bool horizontal)
        {
            if (horizontal && platform0.X < platform1.X + platform1.Width && platform1.X < platform0.X + platform0.Width)
            {
                return true;
            }

            if (!horizontal && platform0.Y < platform1.Y + platform1.Height && platform1.Y < platform0.Y + platform0.Height)
            {
                return true;
            }

            return false;
        }
    }
}
