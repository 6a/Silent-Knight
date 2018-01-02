using Localisation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum BONUS
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
    DODGE_CHANCE
}

public abstract class Bonus
{
    public int m_times;
    public int m_limit;
    public string m_suffix;

    public abstract float Get(float current);
    public abstract float GetNext(float current);

    public void Increase(int times = 1)
    {
        if (!Maxed())
        {
            m_times += times;
        }

        throw new InvalidOperationException("Cannot increase this bonus any further");
    }

    public bool Maxed()
    {
        if (m_limit == 0) return false;
        return (m_times == m_limit);
    }
}

public class PercentBonus : Bonus
{
    // as a fraction of 1
    float m_percentagePerLevel;

    public PercentBonus(float percentagePerLevel, int times, int limit)
    {
        m_percentagePerLevel = percentagePerLevel;
        m_times = times;
        m_limit = limit;
    }

    public override float Get(float current)
    {
        return current * (1 + (m_percentagePerLevel * m_times));
    }

    public override float GetNext(float current)
    {
        return current * (1 + (m_percentagePerLevel * (m_times + 1)));
    }

    public int GetIncreaseFactor(bool next = false)
    {
        if (next) return Mathf.FloorToInt(((m_percentagePerLevel * (m_times + 1)) * 100));
        else return Mathf.FloorToInt(((m_percentagePerLevel * (m_times)) * 100));
    }
}

public class FlatBonus : Bonus
{
    float m_increasePerLevel;

    public FlatBonus(float increasePerLevel, int times, int limit)
    {
        m_increasePerLevel = increasePerLevel;
        m_times = times;
        m_limit = limit;
    }

    public override float Get(float current)
    {
        return current + (m_increasePerLevel * m_times);
    }

    public override float GetNext(float current)
    {
        return current + (m_increasePerLevel * (m_times + 1));
    }
}

public class BonusManager : MonoBehaviour
{
    [Tooltip("In this order: crit+ | dmg+ | ultdur+ | cdkick- | cdspin- | cdbash- | cddeflect- | cdult- | health+ | dodge+")]
    [SerializeField] DynamicTextField[] m_percentBonusField = new DynamicTextField[10];

    List<Bonus> m_bonuses = new List<Bonus>();

    static BonusManager m_instance;

    void Awake ()
    {
        m_instance = this;

        // crit+
        var fb = new FlatBonus(1, PPM.LoadInt(PPM.KEY_INT.BONUS_CRIT_CHANCE), 40);
        fb.m_suffix = "%";
        m_bonuses.Add(fb);

        // damage+
        var pb = new PercentBonus(0.01f, PPM.LoadInt(PPM.KEY_INT.BONUS_ATTACK_DAMAGE), 0);
        pb.m_suffix = "%";
        m_bonuses.Add(pb);

        // ulti duration+
        fb = new FlatBonus(1, PPM.LoadInt(PPM.KEY_INT.BONUS_ULT_DUR), 15);
        fb.m_suffix = "s";
        m_bonuses.Add(fb);

        // kick cooldown-
        pb = new PercentBonus(-0.025f, PPM.LoadInt(PPM.KEY_INT.BONUS_KICK_CD), 16);
        pb.m_suffix = "s";
        m_bonuses.Add(pb);

        // spin cooldown-
        pb = new PercentBonus(-0.025f, PPM.LoadInt(PPM.KEY_INT.BONUS_SPIN_CD), 16);
        pb.m_suffix = "s";
        m_bonuses.Add(pb);

        // bash cooldown-
        pb = new PercentBonus(-0.025f, PPM.LoadInt(PPM.KEY_INT.BONUS_BASH_CD), 16);
        pb.m_suffix = "s";
        m_bonuses.Add(pb);

        // deflect cooldown-
        pb = new PercentBonus(-0.025f, PPM.LoadInt(PPM.KEY_INT.BONUS_DEFLECT_CD), 16);
        pb.m_suffix = "s";
        m_bonuses.Add(pb);

        // ult cooldown-
        pb = new PercentBonus(-0.025f, PPM.LoadInt(PPM.KEY_INT.BONUS_ULT_CD), 20);
        pb.m_suffix = "s";
        m_bonuses.Add(pb);

        // health+
        pb = new PercentBonus(0.01f, PPM.LoadInt(PPM.KEY_INT.BONUS_HEALTH), 0);
        pb.m_suffix = "hp";
        m_bonuses.Add(pb);

        // dodge+
        fb = new FlatBonus(1, PPM.LoadInt(PPM.KEY_INT.BONUS_DODGE_CHANCE), 50);
        fb.m_suffix = "%";
        m_bonuses.Add(fb);
    }

    public static void UpdateShopDisplay(BONUS bonus, JPlayerUnit playerRef)
    {
        if (m_instance.m_bonuses[(int)bonus].Maxed())
        {
            m_instance.m_percentBonusField[(int)bonus].Disable();
        }
        else
        {
            string currentValue = string.Empty;
            string nextValue = string.Empty;
            if (bonus == BONUS.DAMAGE_BOOST)
            {
                currentValue = GetIncreaseFactor(bonus) + m_instance.m_bonuses[(int)bonus].m_suffix;
                nextValue = GetIncreaseFactor(bonus, true) + m_instance.m_bonuses[(int)bonus].m_suffix;
            }
            else
            {
                print(GetModifiedValue(bonus, playerRef.GetValue(bonus)));
                currentValue = Math.Round(GetModifiedValue(bonus, playerRef.GetValue(bonus)), 1) + m_instance.m_bonuses[(int)bonus].m_suffix;
                nextValue = Math.Round(GetModifiedValue(bonus, playerRef.GetValue(bonus), true), 1) + m_instance.m_bonuses[(int)bonus].m_suffix;
            }

            m_instance.m_percentBonusField[(int)bonus].Value1 = currentValue;
            m_instance.m_percentBonusField[(int)bonus].Value2 = nextValue;
            m_instance.m_percentBonusField[(int)bonus].UpdateLanguage();
        }
    }

    public static void IncreaseBonus(BONUS bonus)
    {
        m_instance.m_bonuses[(int)bonus].Increase();
    }

    public static float GetModifiedValue(BONUS bonus, float rawValue, bool next = false)
    {
        if (next) return m_instance.m_bonuses[(int)bonus].GetNext(rawValue);
        return m_instance.m_bonuses[(int)bonus].Get(rawValue);
    }

    // Warning - only use for attack bonus
    public static int GetIncreaseFactor(BONUS bonus, bool next = false)
    {
        if (next) return (m_instance.m_bonuses[(int)bonus] as PercentBonus).GetIncreaseFactor(true);
        return (m_instance.m_bonuses[(int)bonus] as PercentBonus).GetIncreaseFactor(false);
    }
}
