using UnityEngine;
using System.Collections.Generic;
using Delaunay;

namespace DungeonGeneration
{
    public static class Generator
    {
        static int m_width, m_height;
        static PlatformProperties m_platformProperties;
        static int m_cycles;
        static int m_seed;
        static int m_padding;
        static int m_minPlatforms;
        static char m_emptyChar;
        static char m_platformChar;
        static char m_nodeChar;
        static char m_pathChar;

        static List<Platform> m_platforms;
        static List<Path> m_paths;
        static List<Platform> m_entryPoints;

        public static Dungeon CurrentDungeon { get; set; }
        public static Texture CurrentDungeonTexture
        {
            get { return DungeonToTexture(); }
            set { }
        }

        public static void Init(int width, int height, PlatformProperties platformProperties, 
            int cycles, int padding, int minPlatforms, char emptyChar, char platformChar, char nodeChar, char pathChar)
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
            m_platforms = new List<Platform>();
            m_paths = new List<Path>();
        }

        public static void GenerateNewDungeon(int seed = 0)
        {
            m_seed = seed;
            Random.InitState(m_seed);
            Generate();
        }

        private static void Generate ()
        {
            // Platforms

            m_platforms.Clear();

            for (int i = 0; i < m_cycles; i++)
            {
                var platform = GeneratePlatform(i);

                bool isValid = true;

                foreach (var p in m_platforms)
                {
                    if (p.Intersects(platform, m_padding))
                    {
                        isValid = false;
                    }
                }

                if (isValid)
                {
                    m_platforms.Add(platform);
                }
            }

            List<Vector2> centers = new List<Vector2>();
            List<uint> colors = new List<uint>();
            foreach (var platform in m_platforms)
            {
                colors.Add(0);
                centers.Add(platform.Center);
            }

            // Paths
            Voronoi voronoi = new Voronoi(centers, colors, new Rect(0, 0, m_width, m_height));

            var minSpanningTree = voronoi.SpanningTree(KruskalType.MINIMUM);

            m_paths.Clear();

            foreach (var line in minSpanningTree)
            {
                var platform0 = m_platforms.Find(i => i.Center == line.p0.Value);
                platform0.Connections++;

                var platform1 = m_platforms.Find(i => i.Center == line.p1.Value);
                platform1.Connections++;

                var dir = new Vector2(platform1.X, platform1.Y) - new Vector2(platform0.X, platform0.Y);

                dir = Snap(dir);

                if (dir == Vector2.up)
                {
                    if (IsWithinBounds(platform0, platform1, true))
                    {
                        var minx = (platform0.X >= platform1.X) ? platform0.X : platform1.X;
                        var maxx = (platform0.X + platform0.Width <= platform1.X + platform1.Width) ? platform0.X + platform0.Width : platform1.X + platform1.Width;
                        var randX = Random.Range(minx, maxx);

                        var pathStart = new Vector2(randX, platform0.Y + platform0.Height);

                        var pathEnd = new Vector2(randX, platform1.Y);

                        m_paths.Add(new Path(pathStart, pathEnd));
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

                        m_paths.Add(new Path(pathStart, branchStart, startVec, branchVec));
                    }
                }
                else if (dir == Vector2.down)
                {
                    // There is never a case where the path is downwards, probably due to the way that the Delauney library organises it's points internally.
                    // (At least not within the first 5000 seeds).
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

                        m_paths.Add(new Path(pathStart, pathEnd));
                    }
                    else
                    {
                        Vector2 pathStart, branchStart;
                        Vector2 startVec, branchVec;

                        pathStart = new Vector2(platform0.X - 1, platform0.Y + platform0.Height - 1);
                        branchStart = new Vector2(platform1.X + platform1.Width - 1, platform0.Y + platform0.Height);
                        branchVec = new Vector2(0, platform1.Y - (platform0.Y + platform0.Height));

                        startVec = new Vector2(branchStart.x - pathStart.x - 1, 0);

                        m_paths.Add(new Path(pathStart, branchStart, startVec, branchVec));
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

                        m_paths.Add(new Path(pathStart, pathEnd));
                    }
                    else
                    {
                        Vector2 pathStart, branchStart;
                        Vector2 startVec, branchVec;

                        pathStart = new Vector2(platform0.X + platform0.Width, platform0.Y + platform0.Height - 1);
                        branchStart = new Vector2(platform1.X, platform0.Y + platform0.Height - 1);
                        branchVec = new Vector2(0, platform1.Y - (platform0.Y + platform0.Height - 1));

                        startVec = new Vector2(branchStart.x - pathStart.x, 0);

                        m_paths.Add(new Path(pathStart, branchStart, startVec, branchVec));
                    }
                }

                //Debug.DrawLine(new Vector3(line.p0.Value.x, 0, line.p0.Value.y), new Vector3(line.p1.Value.x, 0, line.p1.Value.y), Color.green, 1000);
            }   

            var map = Serialise();

            CurrentDungeon = new Dungeon(map);
        }

        private static Platform GeneratePlatform(int id)
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

        private static Dungeon Serialise()
        {
            var map = new List<string>();

            string line = string.Empty;

            for (int i = 0; i < m_width; i++)
            {
                line += m_emptyChar;
            }

            for (int i = 0; i < m_height; i++)
            {
                map.Add(line);
            }

            foreach (var platform in m_platforms)
            {
                for (int i = 0; i < platform.Height; i++)
                {
                    string str = map[i + platform.Y];
                    for (int j = 0; j < platform.Width; j++)
                    {
                        var c = platform.IsNode() ? m_nodeChar.ToString() : m_platformChar.ToString();

                        str = str.Remove(j + platform.X, 1).Insert(j + platform.X, c);
                    }

                    map[i + platform.Y] = str;
                }
            }

            foreach (var path in m_paths)
            {
                if (path.IsStraight)
                {
                    for (int i = 0; i < path.StartVector.magnitude; i++)
                    {
                        var coord = path.Origin + path.StartVector.normalized * i;

                        map[(int)coord.y] = map[(int)coord.y].Remove((int)coord.x, 1).Insert((int)coord.x, m_pathChar.ToString());
                    }
                }
                else
                {
                    for (int i = 0; i < path.StartVector.magnitude; i++)
                    {
                        var coord = path.Origin + path.StartVector.normalized * i;

                        map[(int)coord.y] = map[(int)coord.y].Remove((int)coord.x, 1).Insert((int)coord.x, m_pathChar.ToString());
                    }

                    for (int i = 0; i < Mathf.Abs(path.BranchVector.magnitude); i++)
                    {
                        var coord = path.Branch + path.BranchVector.normalized * i;
                        map[(int)coord.y] = map[(int)coord.y].Remove((int)coord.x, 1).Insert((int)coord.x, m_pathChar.ToString());
                    }
                }
            }
            
            // Create new dungeon object and return
            return new Dungeon(map);
        }

        private static Texture DungeonToTexture()
        {
            var texture = new Texture2D(CurrentDungeon[0].Length, CurrentDungeon.Count, TextureFormat.ARGB32, false, false);

            for (int i = 0; i < CurrentDungeon.Count; i++)
            {
                for (int j = 0; j < CurrentDungeon[0].Length; j++)
                {
                    texture.SetPixel(CurrentDungeon[0].Length - j - 1, CurrentDungeon.Count - i - 1, CharToColor(CurrentDungeon[i][j]));
                }
            }
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            return texture;
        }

        private static Color CharToColor(char c)
        {
            if (c == m_emptyChar) return Color.gray;
            if (c == m_platformChar) return Color.black;
            if (c == m_pathChar) return Color.blue;
            if (c == m_nodeChar) return Color.red;

            return new Color(1, 0, 1);
        }

        // Takes a direction vector and snaps it to NESorW
        private static Vector2 Snap(Vector2 v)
        {
            v = v.normalized;

            var Y = v.y;
            var X = v.x;

            if (Mathf.Abs(Y) >= Mathf.Abs(X))
            {
                if (Y > 0)
                {
                    return Vector2.up;
                }
                else
                {
                    return Vector2.down;
                }
            }
            else
            {
                if (X > 0)
                {
                    return Vector2.right;
                }
                else
                {
                    return Vector2.left;
                }
            }
        }

        private static bool IsWithinBounds(Platform platform0, Platform platform1, bool horizontal)
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
