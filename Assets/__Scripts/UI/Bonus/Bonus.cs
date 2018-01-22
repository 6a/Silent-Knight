using System;
using SilentKnight.Utility;

namespace SilentKnight.UI.Bonus
{
    /// <summary>
    /// Base class for all bonus types. Contains information about, and tools for, different types of bonuses.
    /// </summary>
    public abstract class Bonus
    {
        // Number of times that this bonus has been increased.
        public int Increments;

        // Maximum number of times that this bonus can be increased.
        public int Limit;

        // String suffix for this bonus, for formatting strings.
        public string Suffix;

        // Abstract functions to be overriden by derived classes.
        public abstract float Get(float current);
        public abstract float GetNext(float current);
        public abstract float GetDecimal(float current);

        /// <summary>
        /// Increases the number of times that this bonus has been incremented by the argument passed in. Can be negative.
        /// </summary>
        public void Add(int times = 1)
        {
            // Clamp the modification to ensure that the bonus can only be shifted up or down by a maximum of 1.
            UnityEngine.Mathf.Clamp(times, -1, 1);

            if (times + Increments < 0 || (times + Increments > Limit && Limit != 0))
            {
                // If the modification pushes the value outside of the accepted limit, throw an exception.
                // This is to prevent any abuse of the bonus system, providing some protection against 
                // incorrect/manipulated data affecting the hi-score system.
                throw new InvalidOperationException("Cannot modify this bonus any further");
            }

            Increments += times;
        }

        /// <summary>
        /// Returns the current state of this bonus.
        /// </summary>
        public Enums.BONUS_STATE State()
        {
            if (Increments == 0) return Enums.BONUS_STATE.AT_MINIMUM;
            else if (Limit == 0 || Increments < Limit) return Enums.BONUS_STATE.VALID;
            else return Enums.BONUS_STATE.AT_MAXIMUM;
        }
    }
}
