using UnityEngine;
using SilentKnight.Entities;
using SilentKnight.PathFinding;

namespace SilentKnight.AI
{
    /// <summary>
    /// Handles management of AI units: Platform allocation, Activation, and Animation pausing.
    /// </summary>
    public class AIManager : MonoBehaviour
    {
        AISet m_enemySet;
        static AIManager m_instance;

        void Awake()
        {
            m_instance = this;
        }

        /// <summary>
        /// Load information about what enemy units are on which platforms.
        /// </summary>
        public static void LoadAIData(AISet data)
        {
            m_instance.m_enemySet = data;
        }

        /// <summary>
        /// Activates all the units on a particular platform.
        /// </summary>
        public static void ActivateUnits(int platformID)
        {
            foreach (var enemy in m_instance.m_enemySet.Enemies[platformID])
            {
                if (enemy == null) continue;
                var enemyObj = (PathFindingObject)enemy;
                if (!enemyObj.IsDead) enemyObj.Running = true;
            }
        }

        /// <summary>
        /// Pauses the Animations for all units in the dungeon
        /// </summary>    
        public static void PauseUnits()
        {
            // Note: Iterates through all registered enemies and disables the attached animator component.
            foreach (var enemyset in m_instance.m_enemySet.Enemies)
            {
                foreach (var enemy in enemyset.Value)
                {
                    if (enemy == null) continue;
                    ((PathFindingObject)enemy).GetComponent<Animator>().enabled = false;

                }
            }
        }

        /// <summary>
        /// Unpauses the Animations for all units in the dungeon
        /// </summary>    
        public static void UnPauseUnits()
        {
            // Note: Iterates through all registered enemies and enables the attached animator component.
            foreach (var enemyset in m_instance.m_enemySet.Enemies)
            {
                foreach (var enemy in enemyset.Value)
                {
                    if (enemy == null) continue;
                    ((PathFindingObject)enemy).GetComponent<Animator>().enabled = true;

                }
            }
        }

        /// <summary>
        /// Removes a unit from the datastore. Remember to delete the object afterwards!
        /// </summary>
        public static void RemoveUnit(int platformID, IAttackable unit)
        {
            m_instance.m_enemySet.GetEnemies(platformID).Remove(unit);
        }

        /// <summary>
        /// Returns a reference to the closest enemy unit for a particular platform.
        /// </summary>
        public static IAttackable GetNewTarget(Vector3 playerPosition, int platformID)
        {
            var potentialTargets = m_instance.m_enemySet.GetEnemies(platformID);

            if (potentialTargets == null || potentialTargets.Count == 0) return null;
            IAttackable closestUnit = null;

            float minDistance = float.MaxValue;

            foreach (var targ in potentialTargets)
            {
                if (targ == null) continue;

                var dist = Vector3.Distance(targ.GetPosition(), playerPosition);

                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestUnit = targ;
                }
            }

            return closestUnit;
        }
    }
}
