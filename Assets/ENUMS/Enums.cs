using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Enum helper class.
/// </summary>
public class Enums
{
    public static PLAYER_STAT ConvertAttackToBonus(PLAYER_ATTACK a)
    {
        switch (a)
        {
            case PLAYER_ATTACK.SWORD_SPIN:  return PLAYER_STAT.COOLDOWN_SPIN;
            case PLAYER_ATTACK.KICK:        return PLAYER_STAT.COOLDOWN_KICK;
            case PLAYER_ATTACK.SHIELD:      return PLAYER_STAT.COOLDOWN_SHIELD_BASH;
            case PLAYER_ATTACK.DEFLECT:     return PLAYER_STAT.COOLDOWN_DEFLECT;
            case PLAYER_ATTACK.ULTIMATE:    return PLAYER_STAT.COOLDOWN_ULT;
        }

        return PLAYER_STAT.NULL;
    }

    // WARNING: DO NOT RE-ORDER THESE ENUMS. IF YOU WANT TO ADD TO THEM, JUST APPEND THE RANGE.

    /// <summary>
    /// Represents different afflications.
    /// </summary>
    public enum AFFLICTION { NONE, STUN, SLOW, CONFUSE, FLINCH }

    /// <summary>
    /// Represents various different animations.
    /// </summary>
    public enum ANIMATION { ATTACK_BASIC1, ATTACK_BASIC2, ATTACK_ULTIMATE, ATTACK_KICK, ATTACK_SHIELD, DEFLECT, BUFF, DEATH, JUMP }

    /// <summary>
    /// Represents the current state of a bonus statistic.
    /// </summary>
    public enum BONUS_STATE { AT_MINIMUM, VALID, AT_MAXIMUM, INVALID }

    /// <summary>
    /// Represents different types of enemies.
    /// </summary>
    public enum ENEMY_TYPE { AXE, SPEAR, DAGGER, BOW, PALADIN }

    /// <summary>
    /// Represents player attacks.
    /// </summary>
    public enum PLAYER_ATTACK { SWORD_SPIN, KICK, SHIELD, DEFLECT, ULTIMATE }

    /// <summary>
    /// Represents different bonus statistic properties.
    /// </summary>
    public enum PLAYER_STAT
    {
        CHANCE_CRIT,
        BOOST_DAMAGE,
        DURATION_INCREASE_ULTIMATE,
        COOLDOWN_KICK,
        COOLDOWN_SPIN,
        COOLDOWN_SHIELD_BASH,
        COOLDOWN_DEFLECT,
        COOLDOWN_ULT,
        BOOST_HEALTH,
        CHANCE_DODGE,
        NULL
    }

    /// <summary>
    /// Represents different audio channels for this project.
    /// </summary>
    public enum AUDIO_CHANNEL { MASTER, BGM, SFX }

    /// <summary>
    /// Represents different types of sound effects.
    /// </summary>
    public enum SFX_TYPE { SWORD_IMPACT, FOOTSTEP, ENEMY_ATTACK_IMPACT, KICK, SHIELD_SLAM, DEFLECT, BIG_IMPACT }

    /// <summary>
    /// Represents the various variations of the background music.
    /// </summary>
    public enum BGM_VARIATION { QUIET, LOUD }

    /// <summary>
    /// Represents the various graphics levels.
    /// </summary>
    public enum GFX_QUALITY { LOW, MID, HIGH };
}