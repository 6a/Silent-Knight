using System.Collections.Generic;
using UnityEngine;

namespace DungeonGeneration
{
    public class Dungeon : List<string>
    {
        public List<Vector2> PlatformCentres { get; set; }

        public Dungeon(List<string> data)
        {   
            PlatformCentres = new List<Vector2>();
            foreach (var line in data)
            {
                Add(line);
            }
        }
    }
}