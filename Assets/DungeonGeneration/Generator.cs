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
        static char m_pathChar;
        static List<Platform> m_platforms;
        static List<Path> m_paths;

        public static Dungeon CurrentDungeon { get; set; }
        public static Texture CurrentDungeonTexture
        {
            get { return DungeonToTexture(); }
            set { }
        }

        public static void Init(int width, int height, PlatformProperties platformProperties, 
            int cycles, int padding, int minPlatforms, char emptyChar, char platformChar, char pathChar)
        {
            m_width = width;
            m_height = height;
            m_platformProperties = platformProperties;
            m_cycles = cycles;
            m_padding = padding;
            m_minPlatforms = minPlatforms;
            m_emptyChar = emptyChar;
            m_platformChar = platformChar;
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
                var platform1 = m_platforms.Find(i => i.Center == line.p1.Value);

                // if a straigth path can be found, add that to the list.

                // if no straight path is found, create a bendy path

                // m_paths.Add(new Path())
                Debug.DrawLine(new Vector3(line.p0.Value.x, 0, line.p0.Value.y), new Vector3(line.p1.Value.x, 0, line.p1.Value.y), Color.green, 1000);
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
            x = Random.Range(0, m_width - w);
            y = Random.Range(0, m_height - h);
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
                        str = str.Remove(j + platform.X, 1).Insert(j + platform.X, m_platformChar.ToString());
                    }

                    map[i + platform.Y] = str;
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
                    Debug.Log((CurrentDungeon.Count - i - 1) + " | " + (CurrentDungeon[0].Length - j - 1));
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

            return new Color(1, 0, 1);
        }
    }
}
