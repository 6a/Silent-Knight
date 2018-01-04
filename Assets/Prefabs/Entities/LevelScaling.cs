
namespace Entities
{
    /// <summary>
    /// XP FORMULA: 
    /// @CONSTANT = 0.05
    /// @LEVEL = 1 + CONST * SQRT(XP)
    /// @XP = ((LEVEL - 1) / CONST)^2
    /// </summary>
    public static class LevelScaling
    {
        const float CONST = 0.05f;
        const int CREDITS_PER_LEVEL = 3;

        public static int GetLevel(int xp)
        {
            return 1 + UnityEngine.Mathf.FloorToInt(CONST * UnityEngine.Mathf.Sqrt(xp));
        }

        public static int GetXP(int level)
        {
            return UnityEngine.Mathf.FloorToInt(UnityEngine.Mathf.Pow(((level - 1) / CONST), 2));
        }

        public static int GetScaledHealth (int level, int baseHealth)
        {
            return baseHealth + ((level - 1) * baseHealth) / 2;
        }

        public static int GetScaledDamage(int level, int baseDamage)
        {
            return baseDamage + ((level - 1) * baseDamage) / 2;
        }

        public static int CreditsPerLevel
        {
            get
            {
                return CREDITS_PER_LEVEL;
            }
        }
    }
}
