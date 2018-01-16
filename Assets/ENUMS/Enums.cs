using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Enum helper class
/// </summary>
public class Enums
{
    public static PROPERTY ConvertAttackToBonus(PLAYER_ATTACK a)
    {
        switch (a)
        {
            case PLAYER_ATTACK.SWORD_SPIN: return PROPERTY.CD_SPIN;
            case PLAYER_ATTACK.KICK: return PROPERTY.CD_KICK;
            case PLAYER_ATTACK.SHIELD: return PROPERTY.CD_SHIELD_BASH;
            case PLAYER_ATTACK.DEFLECT: return PROPERTY.CD_DEFLECT;
            case PLAYER_ATTACK.ULTIMATE: return PROPERTY.CD_ULT;
        }

        return PROPERTY.NULL;
    }

    /// <summary>
    /// Represents player attacks
    /// </summary>
    public enum PLAYER_ATTACK { SWORD_SPIN, KICK, SHIELD, DEFLECT, ULTIMATE }
}


