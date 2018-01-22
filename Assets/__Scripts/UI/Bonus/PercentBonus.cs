using UnityEngine;
using System;

namespace SilentKnight.UI.Bonus
{
    /// <summary>
    /// Represents a flat bonus. Derives from abstract class 'Bonus'.
    /// </summary>
    public class PercentBonus : Bonus
    {
        // The amount by which 1 point increases the value for this bonus, as a percentage represented by a value between 0-1.
        float m_percentagePerLevel;

        public PercentBonus(float percentagePerLevel, int times, int limit)
        {
            m_percentagePerLevel = percentagePerLevel;
            Increments = times;
            Limit = limit;
        }

        /// <summary>
        /// Returns the total bonus as a value of multiplier (1 + bonus).
        /// </summary>
        public override float Get(float current)
        {
            return current * (1 + (m_percentagePerLevel * Increments));
        }

        /// <summary>
        /// Returns the total bonus for the next level, as a value of multiplier (1 + bonus).
        /// </summary>
        public override float GetNext(float current)
        {
            return current * (1 + (m_percentagePerLevel * (Increments + 1)));
        }

        /// <summary>
        /// Returns the increase factor as a flat integer out of 100. 0 = 0%, 50 = 50% etc. If next is false, it returns the 
        /// current level. Otherwise it will return data for the next level.
        /// </summary>
        public int GetIncreaseFactor(bool next = false)
        {
            if (next) return Mathf.FloorToInt(((m_percentagePerLevel * (Increments + 1)) * 100));
            else return Mathf.FloorToInt(((m_percentagePerLevel * (Increments)) * 100));
        }

        /// <summary>
        /// Do not use this function for percent bonus! This function will throw an exception.
        /// </summary>
        public override float GetDecimal(float current)
        {
            throw new NotImplementedException();
        }
    }
}