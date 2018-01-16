using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Enum helper class
/// </summary>
public class Enums
{
    public static PLAYER_STAT ConvertAttackToBonus(PLAYER_ATTACK a)
    {
        switch (a)
        {
            case PLAYER_ATTACK.SWORD_SPIN:  return PLAYER_STAT.CD_SPIN;
            case PLAYER_ATTACK.KICK:        return PLAYER_STAT.CD_KICK;
            case PLAYER_ATTACK.SHIELD:      return PLAYER_STAT.CD_SHIELD_BASH;
            case PLAYER_ATTACK.DEFLECT:     return PLAYER_STAT.CD_DEFLECT;
            case PLAYER_ATTACK.ULTIMATE:    return PLAYER_STAT.CD_ULT;
        }

        return PLAYER_STAT.NULL;
    }

    /// <summary>
    /// Represents different afflications
    /// </summary>
    public enum AFFLICTION { NONE, STUN, SLOW, CONFUSE, FLINCH }

    /// <summary>
    /// Represents the current state of a bonus statistic
    /// </summary>
    public enum BONUS_STATE { AT_MINIMUM, VALID, AT_MAXIMUM, INVALID }

    /// <summary>
    /// Represents different types of enemies
    /// </summary>
    public enum ENEMY_TYPE { AXE, SPEAR, DAGGER, BOW, PALADIN }

    /// <summary>
    /// Represents player attacks
    /// </summary>
    public enum PLAYER_ATTACK { SWORD_SPIN, KICK, SHIELD, DEFLECT, ULTIMATE }

    /// <summary>
    /// Represents different bonus statistic properties
    /// </summary>
    public enum PLAYER_STAT
    {
        CRIT_CHANCE,
        DAMAGE_BOOST,
        ULT_DURATION_INCREASE,
        CD_KICK,
        CD_SPIN,
        CD_SHIELD_BASH,
        CD_DEFLECT,
        CD_ULT,
        HEALTH_BOOST,
        DODGE_CHANCE,
        NULL
    }


}


