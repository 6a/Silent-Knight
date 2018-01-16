using Localisation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public enum PROPERTY
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

public enum BONUS_STATE { AT_MINIMUM, VALID, AT_MAXIMUM, INVALID }

public abstract class Bonus
{
    public int m_times;
    public int m_limit;
    public string m_suffix;

    public abstract float Get(float current);
    public abstract float GetNext(float current);
    public abstract float GetDecimal(float current);

    public void Modify(int times = 1)
    {
        if (times + m_times < 0 || (times + m_times > m_limit && m_limit != 0))
        {
            throw new InvalidOperationException("Cannot modify this bonus any further");
        }

        m_times += times;
    }

    public BONUS_STATE State()
    {
        if (m_times == 0) return BONUS_STATE.AT_MINIMUM;
        else if (m_limit == 0 || m_times < m_limit) return BONUS_STATE.VALID;
        else return BONUS_STATE.AT_MAXIMUM;
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

    public override float GetDecimal(float current)
    {
        throw new NotImplementedException();
    }
}

public class FlatBonus : Bonus
{
    public float m_increasePerLevel;

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

    public override float GetDecimal(float current)
    {
        return current + ((m_times * m_increasePerLevel) / 100f);
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
    [SerializeField] Text[] m_statText = new Text[10];
    [SerializeField] Text m_currentCreditsText, m_spentCreditsText;
    [SerializeField] GameObject[] m_saveResetButtons = new GameObject[2];
    [SerializeField] LinkButton m_linkButton;

    List<Bonus> m_bonuses = new List<Bonus>();

    int m_currentCredits;
    int m_spentCredits;
    short[] m_changeVector;

    static BonusManager m_instance;

    void Awake ()
    {
        m_instance = this;
        m_currentCredits = PersistentData.LoadInt(PersistentData.KEY_INT.CURRENT_CREDITS);
        m_spentCredits = PersistentData.LoadInt(PersistentData.KEY_INT.SPENT_CREDITS);

        //PPM.SaveInt(PPM.KEY_INT.CURRENT_CREDITS, 0);

        UpdateCreditDisplay();
        InitBonusMatrix();
        m_changeVector = new short[10];
    }

    bool ChangesMade ()
    {
        foreach (var val in m_changeVector)
        {
            if (val != 0) return true;
        }
        return true;
    }

    private void UpdateCreditDisplay()
    {
        var col = (m_currentCredits == 0) ? Color.red : new Color(0, 242f / 255f, 1);

        m_currentCreditsText.text = LocalisationManager.GetCurrency() + m_currentCredits.ToString();
        m_currentCreditsText.color = col;

        col = (m_spentCredits == 0) ? Color.red : new Color(0, 242f / 255f, 1);
        m_spentCreditsText.text = m_spentCredits.ToString();
        m_spentCreditsText.color = col;

        if (m_currentCredits > 0) m_linkButton.Toggle(true);
        else m_linkButton.Toggle(false);
    }

    private void InitBonusMatrix()
    {
        m_bonuses = new List<Bonus>();

        // crit+
        var fb = new FlatBonus(1, PersistentData.LoadInt(PersistentData.KEY_INT.BONUS_CRIT_CHANCE), 70) { m_suffix = "%" };
        m_bonuses.Add(fb);

        // damage+
        var pb = new PercentBonus(0.01f, PersistentData.LoadInt(PersistentData.KEY_INT.BONUS_ATTACK_DAMAGE), 0) { m_suffix = "%" };
        m_bonuses.Add(pb);

        // ulti duration+
        fb = new FlatBonus(0.5f, PersistentData.LoadInt(PersistentData.KEY_INT.BONUS_ULT_DUR), 30) { m_suffix = "s" };
        m_bonuses.Add(fb);

        // kick cooldown-
        pb = new PercentBonus(-0.025f, PersistentData.LoadInt(PersistentData.KEY_INT.BONUS_KICK_CD), 16) { m_suffix = "s" };
        m_bonuses.Add(pb);

        // spin cooldown-
        pb = new PercentBonus(-0.025f, PersistentData.LoadInt(PersistentData.KEY_INT.BONUS_SPIN_CD), 16) { m_suffix = "s" };
        m_bonuses.Add(pb);

        // bash cooldown-
        pb = new PercentBonus(-0.025f, PersistentData.LoadInt(PersistentData.KEY_INT.BONUS_BASH_CD), 16) { m_suffix = "s" };
        m_bonuses.Add(pb);

        // deflect cooldown-
        pb = new PercentBonus(-0.025f, PersistentData.LoadInt(PersistentData.KEY_INT.BONUS_DEFLECT_CD), 16) { m_suffix = "s" };
        m_bonuses.Add(pb);

        // ult cooldown-
        pb = new PercentBonus(-0.025f, PersistentData.LoadInt(PersistentData.KEY_INT.BONUS_ULT_CD), 20) { m_suffix = "s" };
        m_bonuses.Add(pb);

        // health+
        pb = new PercentBonus(0.01f, PersistentData.LoadInt(PersistentData.KEY_INT.BONUS_HEALTH), 0) { m_suffix = "hp" };
        m_bonuses.Add(pb);

        // dodge+
        fb = new FlatBonus(1, PersistentData.LoadInt(PersistentData.KEY_INT.BONUS_DODGE_CHANCE), 50) { m_suffix = "%" };
        m_bonuses.Add(fb);
    }

    public static void AddCredits(int amount, bool save)
    {
        m_instance.m_currentCredits += amount;
        m_instance.UpdateCreditDisplay();

        if (save) PersistentData.SaveInt(PersistentData.KEY_INT.CURRENT_CREDITS, m_instance.m_currentCredits);
    }

    public static bool CanSubtract(PROPERTY bonus)
    {
        if (m_instance.m_spentCredits == 0 || m_instance.m_bonuses[(int)bonus].State() == BONUS_STATE.AT_MINIMUM) return false;
        return true;
    }

    public static bool CanAdd(PROPERTY bonus)
    {
        if (m_instance.m_currentCredits == 0 || m_instance.m_bonuses[(int)bonus].State() == BONUS_STATE.AT_MAXIMUM) return false;
        return true;
    }

    public static void UpdateBonusDisplay(PROPERTY bonus, PlayerPathFindingObject playerRef)
    {
        m_instance.m_percentBonusField[(int)bonus].SetButtonState(m_instance.m_bonuses[(int)bonus].State());
        if (m_instance.m_currentCredits == 0) m_instance.m_percentBonusField[(int)bonus].HideButton(false);
        if (m_instance.m_spentCredits == 0) m_instance.m_percentBonusField[(int)bonus].HideButton(true);

        string currentValue = string.Empty;
        string nextValue = string.Empty;
        if (bonus == PROPERTY.DAMAGE_BOOST)
        {
            currentValue = GetIncreaseFactor(bonus) + m_instance.m_bonuses[(int)bonus].m_suffix;
            nextValue = GetIncreaseFactor(bonus, true) + m_instance.m_bonuses[(int)bonus].m_suffix;
        }
        else
        {
            currentValue = GetModifiedValue(bonus, playerRef.GetPlayerProperty(bonus)).Format(m_instance.m_bonuses[(int)bonus].m_suffix);
            nextValue = GetModifiedValue(bonus, playerRef.GetPlayerProperty(bonus), true).Format(m_instance.m_bonuses[(int)bonus].m_suffix);
        }

        m_instance.m_percentBonusField[(int)bonus].Value1 = currentValue;
        m_instance.m_statText[(int)bonus].text = currentValue;
        m_instance.m_percentBonusField[(int)bonus].Value2 = nextValue;
        m_instance.m_percentBonusField[(int)bonus].UpdateLanguage();
    }

    public static void UpdateBonusAmount(PROPERTY bonus, int change)
    {
        m_instance.m_changeVector[(int)bonus] += (short)change;

        if (m_instance.ChangesMade())
        {
            m_instance.ToggleSaveResetButtons(true);
        }
        else
        {
            m_instance.ToggleSaveResetButtons(false);
        }

        m_instance.m_bonuses[(int)bonus].Modify(change);

        m_instance.m_currentCredits -= change;
        m_instance.m_spentCredits += change;

        m_instance.UpdateCreditDisplay();
        UpdateBonusDisplay(bonus, FindObjectOfType<PlayerPathFindingObject>());

        if (m_instance.m_currentCredits == 0)
        {
            foreach (var field in m_instance.m_percentBonusField)
            {
                field.HideButton(false);
            }
        }
        else if (m_instance.m_spentCredits == 0)
        {
            foreach (var field in m_instance.m_percentBonusField)
            {
                field.HideButton(true);
            }
        }
        else
        {
            for (int i = 0; i < m_instance.m_percentBonusField.Length; i++)
            {
                m_instance.m_percentBonusField[i].SetButtonState(m_instance.m_bonuses[i].State());
            }
        }
    }

    void ToggleSaveResetButtons(bool on)
    {
        foreach (var button in m_saveResetButtons)
        {
            button.SetActive(on);
        }
    }

    public static float GetModifiedValueFlatAsDecimal(PROPERTY bonus, float rawValue)
    {
        return m_instance.m_bonuses[(int)bonus].GetDecimal(rawValue);
    }

    public static float GetModifiedValue(PROPERTY bonus, float rawValue, bool next = false)
    {
        if (next) return m_instance.m_bonuses[(int)bonus].GetNext(rawValue);
        return m_instance.m_bonuses[(int)bonus].Get(rawValue);
    }

    public static BONUS_STATE GetBonusState(PROPERTY bonus)
    {
        return m_instance.m_bonuses[(int)bonus].State();
    }
    
    public void Save()
    {
        PersistentData.SaveInt(PersistentData.KEY_INT.BONUS_CRIT_CHANCE, m_bonuses[(int)PROPERTY.CRIT_CHANCE].m_times);
        PersistentData.SaveInt(PersistentData.KEY_INT.BONUS_ATTACK_DAMAGE, m_bonuses[(int)PROPERTY.DAMAGE_BOOST].m_times);
        PersistentData.SaveInt(PersistentData.KEY_INT.BONUS_ULT_DUR, m_bonuses[(int)PROPERTY.ULT_DURATION_INCREASE].m_times);
        PersistentData.SaveInt(PersistentData.KEY_INT.BONUS_KICK_CD, m_bonuses[(int)PROPERTY.CD_KICK].m_times);
        PersistentData.SaveInt(PersistentData.KEY_INT.BONUS_SPIN_CD, m_bonuses[(int)PROPERTY.CD_SPIN].m_times);
        PersistentData.SaveInt(PersistentData.KEY_INT.BONUS_BASH_CD, m_bonuses[(int)PROPERTY.CD_SHIELD_BASH].m_times);
        PersistentData.SaveInt(PersistentData.KEY_INT.BONUS_DEFLECT_CD, m_bonuses[(int)PROPERTY.CD_DEFLECT].m_times);
        PersistentData.SaveInt(PersistentData.KEY_INT.BONUS_ULT_CD, m_bonuses[(int)PROPERTY.CD_ULT].m_times);
        PersistentData.SaveInt(PersistentData.KEY_INT.BONUS_HEALTH, m_bonuses[(int)PROPERTY.HEALTH_BOOST].m_times);
        PersistentData.SaveInt(PersistentData.KEY_INT.BONUS_DODGE_CHANCE, m_bonuses[(int)PROPERTY.DODGE_CHANCE].m_times);

        PersistentData.SaveInt(PersistentData.KEY_INT.CURRENT_CREDITS, m_currentCredits);
        PersistentData.SaveInt(PersistentData.KEY_INT.SPENT_CREDITS, m_spentCredits);

        FindObjectOfType<PlayerPathFindingObject>().UpdateHealthCap();

        ToggleSaveResetButtons(false);
        m_changeVector = new short[10];
    }

    public void Reset()
    {
        InitBonusMatrix();

        var playerRef = FindObjectOfType<PlayerPathFindingObject>();

        m_currentCredits = PersistentData.LoadInt(PersistentData.KEY_INT.CURRENT_CREDITS);
        m_spentCredits = PersistentData.LoadInt(PersistentData.KEY_INT.SPENT_CREDITS);
        UpdateCreditDisplay();

        playerRef.UpdateBonusDisplay();

        ToggleSaveResetButtons(false);
        m_changeVector = new short[10];
    }

    // Warning - only use for attack bonus
    public static int GetIncreaseFactor(PROPERTY bonus, bool next = false)
    {
        if (next) return (m_instance.m_bonuses[(int)bonus] as PercentBonus).GetIncreaseFactor(true);
        return (m_instance.m_bonuses[(int)bonus] as PercentBonus).GetIncreaseFactor(false);
    }
}
