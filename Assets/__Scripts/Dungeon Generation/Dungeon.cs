using System.Collections.Generic;
using UnityEngine;

namespace SilentKnight.DungeonGeneration
{
    /// <summary>
    /// Dungeon data storage type. Inherites from string List&lt;string&gt;
    /// </summary>
    public class Dungeon : List<string>
    {
        public List<Platform> Platforms         { get; private set; }
        public List<Path> Paths                 { get; private set; }
        public int Width                        { get; private set; }
        public int Height                       { get; private set; }
        public char EmptyChar                   { get; private set; }
        public char PlatformChar                { get; private set; }
        public char NodeChar                    { get; private set; }
        public char PathChar                    { get; private set; }

        /// <summary>
        /// Returns the dungeon, as a texture file.
        /// </summary>
        public Texture Texture
        {
            get { return DungeonToTexture(); }
            private set { }
        }

        /// <summary>
        /// Returns a list of the nodes within the dungeon.
        /// </summary>
        public List<Platform> Nodes
        {
            get { return GetNodes(); }
            private set { }
        }

        /// <summary>
        /// Will read all dungeon data and serialise it into workable data. Various chars are used to represent the different possible tiles.
        /// </summary>
        public Dungeon(List<Platform> platformData, List<Path> pathData, int width, 
            int height, char emptyChar, char platChar, char nodeChar, char pathChar)
        {
            Platforms = platformData;
            Paths = pathData;
            Height = height;
            Width = width;
            EmptyChar = emptyChar;
            PlatformChar = platChar;
            NodeChar = nodeChar;
            PathChar = pathChar;
            Serialise();
        }

        // Returns all the nodes found in the dungeon. A node is a platform with only 1 path; a start or end node.
        List<Platform> GetNodes()
        {
            return Platforms.FindAll(i => i.IsNode());
        }

        // Returns the dungeon, as a texture.
        Texture DungeonToTexture()
        {
            // Create a blank texture of appropriate size.
            var texture = new Texture2D(this[0].Length, this.Count, TextureFormat.ARGB32, false, false);

            // For ever pixel in the blank texture, write the corresponding tile type as a colour.
            for (int i = 0; i < Count; i++)
            {
                for (int j = 0; j < this[0].Length; j++)
                {
                    texture.SetPixel(this[0].Length - j - 1, Count - i - 1, CharToColor(this[i][j]));
                }
            }

            // Set the texture up so that it can be used.
            texture.filterMode = FilterMode.Point;
            texture.Apply();

            return texture;
        }

        // Convert a char to the corresponding colour for visualisation purposes.
        Color CharToColor(char c)
        {
            // Note: If statements are used as a switch cannot be used for non-constant values, and as the function will exit as soon
            // as it finds a match.

            if (c == EmptyChar)     return Color.gray;
            if (c == PlatformChar)  return Color.black;
            if (c == PathChar)  return Color.blue;
            if (c == NodeChar) return Color.red;

            // If the char doesn't match any of the known chars, a default colour is returned.
            return new Color(1, 0, 1);
        }

        // Converts all the contained dungeon data into strings, and stores them internally.
        void Serialise()
        {
            // Create a new, empty List<string> object.
            var map = new List<string>();

            // Create an empty string and fill it with 0's corresponding to the width of the dungeon, in tiles.
            string line = string.Empty;

            for (int i = 0; i < Width; i++)
            {
                line += EmptyChar;
            }

            // Fill the local container with these blank strings, creating one whole blank tilemap.
            for (int i = 0; i < Height; i++)
            {
                map.Add(line);
            }

            // For each platform stored in this class, convert the coordinate data into chars and insert them
            // into the local container.
            foreach (var platform in Platforms)
            {
                for (int i = 0; i < platform.Height; i++)
                {
                    string str = map[i + platform.Y];
                    for (int j = 0; j < platform.Width; j++)
                    {
                        var c = platform.IsNode() ? NodeChar.ToString() : PlatformChar.ToString();
                        str = str.Remove(j + platform.X, 1).Insert(j + platform.X, c);
                    }

                    map[i + platform.Y] = str;
                }
            }

            // For each path stored in this class, convert the coordinate data into chars and insert them
            // into the local container. The serialisation is slightly different, depending on whether the
            // path is straight or has a bend in it.
            foreach (var path in Paths)
            {
                if (path.IsStraight)
                {
                    for (int i = 0; i < path.StartVector.magnitude; i++)
                    {
                        var coord = path.Origin + path.StartVector.normalized * i;

                        map[(int)coord.y] = map[(int)coord.y].Remove((int)coord.x, 1).Insert((int)coord.x, PathChar.ToString());
                    }
                }
                else
                {
                    for (int i = 0; i < path.StartVector.magnitude; i++)
                    {
                        var coord = path.Origin + path.StartVector.normalized * i;

                        map[(int)coord.y] = map[(int)coord.y].Remove((int)coord.x, 1).Insert((int)coord.x, PathChar.ToString());
                    }

                    for (int i = 0; i < Mathf.Abs(path.BranchVector.magnitude); i++)
                    {
                        var coord = path.Branch + path.BranchVector.normalized * i;
                        map[(int)coord.y] = map[(int)coord.y].Remove((int)coord.x, 1).Insert((int)coord.x, PathChar.ToString());
                    }
                }
            }

            // Write all the serialised data into the internal storage for this class.
            foreach (var row in map)
            {
                Add(row);
            }
        }
    }
}