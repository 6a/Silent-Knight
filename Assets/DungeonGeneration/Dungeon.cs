using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGeneration
{
    public class Dungeon : List<string>
    {
        public List<Platform> Platforms { get; private set; }
        public List<Path> Paths { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Texture Texture
        {
            get { return DungeonToTexture(); }
            private set { }
        }

        public List<Platform> Nodes
        {
            get { return GetNodes(); }
            private set { }
        }

        char m_emptyChar;
        char m_platformChar;
        char m_nodeChar;
        char m_pathChar;

        public Dungeon(List<Platform> platforms, List<Path> paths, int width, int height, char emptyChar, char platChar, char nodeChar, char pathChar)
        {
            Platforms = platforms;
            Paths = paths;
            Height = height;
            Width = width;
            m_emptyChar = emptyChar;
            m_platformChar = platChar;
            m_nodeChar = nodeChar;
            m_pathChar = pathChar;
            Serialise();
        }

        private List<Platform> GetNodes()
        {
            return Platforms.FindAll(i => i.IsNode());
        }

        private Texture DungeonToTexture()
        {
            var texture = new Texture2D(this[0].Length, this.Count, TextureFormat.ARGB32, false, false);

            for (int i = 0; i < Count; i++)
            {
                for (int j = 0; j < this[0].Length; j++)
                {
                    texture.SetPixel(this[0].Length - j - 1, Count - i - 1, CharToColor(this[i][j]));
                }
            }
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            return texture;
        }

        private Color CharToColor(char c)
        {
            if (c == m_emptyChar) return Color.gray;
            if (c == m_platformChar) return Color.black;
            if (c == m_pathChar) return Color.blue;
            if (c == m_nodeChar) return Color.red;

            return new Color(1, 0, 1);
        }

        private void Serialise()
        {
            var map = new List<string>();

            string line = string.Empty;

            for (int i = 0; i < Width; i++)
            {
                line += m_emptyChar;
            }

            for (int i = 0; i < Height; i++)
            {
                map.Add(line);
            }

            foreach (var platform in Platforms)
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

            foreach (var path in Paths)
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

                    for (int i = 0; i < UnityEngine.Mathf.Abs(path.BranchVector.magnitude); i++)
                    {
                        var coord = path.Branch + path.BranchVector.normalized * i;
                        map[(int)coord.y] = map[(int)coord.y].Remove((int)coord.x, 1).Insert((int)coord.x, m_pathChar.ToString());
                    }
                }
            }

            // Pipe data into this objects parent list
            foreach (var row in map)
            {
                Add(row);
            }
        }
    }
}