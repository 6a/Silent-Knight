using SilentKnight.Utility;

namespace SilentKnight.Entities
{
    /// <summary>
    /// Storage class for player cooldown base values.
    /// </summary>
    public class PlayerCooldownConstants
    {
        float[] m_cd;

        public PlayerCooldownConstants(float spinCooldown, float kickCooldown, float shieldCooldown, float reflectCooldown, float ultCooldown)
        {
            m_cd = new float[5];

            m_cd[0] = spinCooldown;
            m_cd[1] = kickCooldown;
            m_cd[2] = shieldCooldown;
            m_cd[3] = reflectCooldown;
            m_cd[4] = ultCooldown;
        }

        /// <summary>
        /// Returns the basecooldown for a particular attack type.
        /// </summary>
        public float Get(Enums.PLAYER_ATTACK attack)
        {
            return m_cd[(int)attack];
        }
    }
}
