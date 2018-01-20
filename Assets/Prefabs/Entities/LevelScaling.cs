namespace Entities
{
    ///<summary>
    /// Helper class for handling level scaling.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public static class LevelScaling
    {
        // Const modifer value for log function.
        const float CONST = 0.05f;

        /// <summary>
        /// Number of credits that the player receives when levelling up.
        /// </summary>
        public static int CreditsPerLevel { get { return 3; }}

        /// <summary>
        /// Returns the level corresponding to the xp passed in as an argument.
        /// </summary>
        /// <remarks>
        /// LEVEL = 1 + CONST * SQRT(XP)
        /// </remarks>
        public static int GetLevel(int xp)
        {
            return 1 + UnityEngine.Mathf.FloorToInt(CONST * UnityEngine.Mathf.Sqrt(xp));
        }

        /// <summary>
        /// Returns the XP corresponding to the level passed in as an argument.
        /// </summary>
        /// <remarks>
        /// XP = ((LEVEL - 1) / CONST)^2
        /// </remarks>
        public static int GetXP(int level)
        {
            return UnityEngine.Mathf.FloorToInt(UnityEngine.Mathf.Pow(((level - 1) / CONST), 2));
        }

        /// <summary>
        /// Returns a scaled health value based on level.
        /// </summary>
        public static int GetScaledHealth (int level, int baseHealth)
        {
            return baseHealth + ((level - 1) * baseHealth) / 2;
        }

        /// <summary>
        /// Returns a scaled damage value based on level.
        /// </summary>
        public static int GetScaledDamage(int level, int baseDamage)
        {
            return baseDamage + ((level - 1) * baseDamage) / 2;
        }
    }
}
