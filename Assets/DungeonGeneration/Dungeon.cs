using System.Collections.Generic;

namespace DungeonGeneration
{
    public class Dungeon : List<string>
    {
        public Dungeon(List<string> data)
        {
            foreach (var line in data)
            {
                Add(line);
            }
        }
    }
}