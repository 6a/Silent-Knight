using System.Collections.Generic;
using SilentKnight.Entities;

namespace SilentKnight.AI
{
    /// <summary>
    /// Storage class for Enemy unit references, per platform.
    /// </summary>
    public class AISet
    {
        public Dictionary<int, List<IAttackable>> Enemies { get; set; }

        public AISet(Dictionary<int, List<IAttackable>> enemies)
        {
            Enemies = enemies;
        }

        /// <summary>
        /// Returns a List of the enemies on a particular platform.
        /// </summary>
        public List<IAttackable> GetEnemies(int platformID)
        {
            if (!Enemies.ContainsKey(platformID)) return null;

            return Enemies[platformID];
        }

        /// <summary>
        /// Removes a unit reference from a particular platform, by IAttackable reference.
        /// </summary>
        public void Remove(int platformID, IAttackable reference)
        {
            Enemies[platformID].Remove(reference);
        }
    }
}
