using Localisation;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles all bonus interactions. Bonus' are stat increases, cooldown reductions, chance modifiers etc.
/// </summary>
public class BonusManager : MonoBehaviour
{
    // References to the text objects that represent each bonus type, in the bonus settings UI Interactive section.
    [Tooltip("In this order: crit+ | dmg+ | ultdur+ | cdkick- | cdspin- | cdbash- | cdreflect- | cdult- | health+ | dodge+")]
    [SerializeField] DynamicTextField[] m_percentBonusField = new DynamicTextField[10];

    // References to the text objects that represent each bonus type, in the bonus settings UI display section.
    [Tooltip("In this order: crit+ | dmg+ | ultdur+ | cdkick- | cdspin- | cdbash- | cdreflect- | cdult- | health+ | dodge+")]
    [SerializeField] Text[] m_statText = new Text[10];

    // References to the text objects that represent current, and spent credits.
    [SerializeField] Text m_currentCreditsText, m_spentCreditsText;

    // References to the save and reset buttons.
    [SerializeField] GameObject[] m_saveResetButtons = new GameObject[2];

    // Reference to the UI button that opens the bonus window.
    [SerializeField] LinkButton m_linkButton;

    // List of all current bonuses.
    List<Bonus> m_bonuses = new List<Bonus>();

    // Storage for current and spent credit counters.
    int m_currentCredits;
    int m_spentCredits;

    // An array of shorts that store the state of change for each bonus.
    short[] m_changeVector;

    static BonusManager m_instance;

    void Awake ()
    {
        m_instance = this;

        // Load data from persistent data.
        m_currentCredits = PersistentData.LoadInt(PersistentData.KEY_INT.CURRENT_CREDITS);
        m_spentCredits = PersistentData.LoadInt(PersistentData.KEY_INT.SPENT_CREDITS);

        // Refresh the window with up-to-date values.
        UpdateCreditDisplay();

        // Initialise the in-memory bonus objects.
        InitBonusMatrix();

        m_changeVector = new short[10];
    }

    /// <summary>
    /// Returns true if any of the bonuses are different from the last time the bonus' were saved.
    /// </summary>
    bool ChangesMade ()
    {
        // If any of the values in the changevector array is not 0, then it can be assumed that
        // a change has been made. An array is used to prevent an addition for one bonus type
        // overwriting a subtraction for another - by using a unique identifier for each bonus,
        // the player can modify multiple bonuses without saving, and the correct state should
        // always be returned by this function.

        foreach (var val in m_changeVector)
        {
            if (val != 0) return true;
        }
        return true;
    }

    /// <summary>
    /// Update the credits display.
    /// </summary>
    void UpdateCreditDisplay()
    {
        // Set the colour based on whether there are any credits or not.
        var col = (m_currentCredits == 0) ? Color.red : new Color(0, 242f / 255f, 1);
        m_currentCreditsText.color = col;

        // Update the text with a string that represents the number of credits, with the appropriate currency character.
        m_currentCreditsText.text = LocalisationManager.GetCurrency() + m_currentCredits.ToString();

        
        // Set the colour based on whether there are any spent credits or not.
        col = (m_spentCredits == 0) ? Color.red : new Color(0, 242f / 255f, 1);
        m_spentCreditsText.color = col;

        // Update the text with a string that represents the number of spent credits.
        m_spentCreditsText.text = m_spentCredits.ToString();

        // If there are any credits available to spend, the in-game UI object will flash to inform the player. Otherwise
        // it is static.
        if (m_currentCredits > 0) m_linkButton.Toggle(true);
        else m_linkButton.Toggle(false);
    }

    /// <summary>
    /// Fill the bonus array with hard-coded data that represents bonus' with appropriate information.
    /// </summary>
    void InitBonusMatrix()
    {
        m_bonuses = new List<Bonus>();

        // crit+
        var fb = new FlatBonus(1, PersistentData.LoadInt(PersistentData.KEY_INT.BONUS_CRIT_CHANCE), 70) { Suffix = "%" };
        m_bonuses.Add(fb);
        
        // damage+
        var pb = new PercentBonus(0.01f, PersistentData.LoadInt(PersistentData.KEY_INT.BONUS_ATTACK_DAMAGE), 0) { Suffix = "%" };
        m_bonuses.Add(pb);

        // ulti duration+
        fb = new FlatBonus(0.5f, PersistentData.LoadInt(PersistentData.KEY_INT.BONUS_ULT_DUR), 30) { Suffix = "s" };
        m_bonuses.Add(fb);

        // kick cooldown-
        pb = new PercentBonus(-0.025f, PersistentData.LoadInt(PersistentData.KEY_INT.BONUS_KICK_CD), 16) { Suffix = "s" };
        m_bonuses.Add(pb);

        // spin cooldown-
        pb = new PercentBonus(-0.025f, PersistentData.LoadInt(PersistentData.KEY_INT.BONUS_SPIN_CD), 16) { Suffix = "s" };
        m_bonuses.Add(pb);

        // bash cooldown-
        pb = new PercentBonus(-0.025f, PersistentData.LoadInt(PersistentData.KEY_INT.BONUS_BASH_CD), 16) { Suffix = "s" };
        m_bonuses.Add(pb);

        // reflect cooldown-
        pb = new PercentBonus(-0.025f, PersistentData.LoadInt(PersistentData.KEY_INT.BONUS_REFLECT_CD), 16) { Suffix = "s" };
        m_bonuses.Add(pb);

        // ult cooldown-
        pb = new PercentBonus(-0.025f, PersistentData.LoadInt(PersistentData.KEY_INT.BONUS_ULT_CD), 20) { Suffix = "s" };
        m_bonuses.Add(pb);

        // health+
        pb = new PercentBonus(0.01f, PersistentData.LoadInt(PersistentData.KEY_INT.BONUS_HEALTH), 0) { Suffix = "hp" };
        m_bonuses.Add(pb);

        // dodge+
        fb = new FlatBonus(1, PersistentData.LoadInt(PersistentData.KEY_INT.BONUS_DODGE_CHANCE), 50) { Suffix = "%" };
        m_bonuses.Add(fb);
    }

    /// <summary>
    /// Add credits to the in-memory storage, and update the window. If 'save' is true, the persistent data will also be updated.
    /// </summary>
    public static void AddCredits(int amount, bool save)
    {
        m_instance.m_currentCredits += amount;
        m_instance.UpdateCreditDisplay();

        if (save) PersistentData.SaveInt(PersistentData.KEY_INT.CURRENT_CREDITS, m_instance.m_currentCredits);
    }

    /// <summary>
    /// Returns true if the bonus represented by the enum argument can be decremented by 1.
    /// </summary>
    public static bool CanSubtract(Enums.PLAYER_STAT bonus)
    {
        if (m_instance.m_spentCredits == 0 || m_instance.m_bonuses[(int)bonus].State() == Enums.BONUS_STATE.AT_MINIMUM) return false;
        return true;
    }

    /// <summary>
    /// Returns true if the bonus represented by the enum argument can be incremented by 1.
    /// </summary>
    public static bool CanAdd(Enums.PLAYER_STAT bonus)
    {
        if (m_instance.m_currentCredits == 0 || m_instance.m_bonuses[(int)bonus].State() == Enums.BONUS_STATE.AT_MAXIMUM) return false;
        return true;
    }

    /// <summary>
    /// Updates the bonus UI window display for a particular bonus. Uses a reference to the player to fish out current values.
    /// </summary>
    public static void UpdateBonusDisplay(Enums.PLAYER_STAT bonus, PlayerPathFindingObject playerRef)
    {
        // Depending on the button state, show or hide the add and minus buttons. These lines here
        // are the only thing stopping the player from attempting to add points past the limit. However,
        // the design of this class should ensure that points can only be added or subtracted 1 per frame.
        // Due to this, no collisions should occur, and disabling the button should be a valid way to prevent
        // additional modifications past the allowed limits.
        m_instance.m_percentBonusField[(int)bonus].SetButtonState(m_instance.m_bonuses[(int)bonus].State());
        if (m_instance.m_currentCredits == 0) m_instance.m_percentBonusField[(int)bonus].HideButton(false);
        if (m_instance.m_spentCredits == 0) m_instance.m_percentBonusField[(int)bonus].HideButton(true);

        // Update the bonus selection window and the information display.
        string currentValue = string.Empty;
        string nextValue = string.Empty;
        if (bonus == Enums.PLAYER_STAT.BOOST_DAMAGE)
        {
            currentValue = GetAttackBonus(bonus) + m_instance.m_bonuses[(int)bonus].Suffix;
            nextValue = GetAttackBonus(bonus, true) + m_instance.m_bonuses[(int)bonus].Suffix;
        }
        else
        {
            currentValue = GetModifiedValue(bonus, playerRef.GetPlayerProperty(bonus)).Format(m_instance.m_bonuses[(int)bonus].Suffix);
            nextValue = GetModifiedValue(bonus, playerRef.GetPlayerProperty(bonus), true).Format(m_instance.m_bonuses[(int)bonus].Suffix);
        }

        m_instance.m_percentBonusField[(int)bonus].Value1 = currentValue;
        m_instance.m_statText[(int)bonus].text = currentValue;
        m_instance.m_percentBonusField[(int)bonus].Value2 = nextValue;
        m_instance.m_percentBonusField[(int)bonus].UpdateLanguage();
    }

    /// <summary>
    /// Update the number of points assigned to a particular bonus.
    /// </summary>
    public static void AddBonusAmount(Enums.PLAYER_STAT bonus, int change)
    {
        // Update the change vector for this particular bonus.
        m_instance.m_changeVector[(int)bonus] += (short)change;

        // If it is found that the current configuration is different from the configuration since the latest
        // saving operation, toggle the save and reset buttons to on. Otherwise turn them off.
        if (m_instance.ChangesMade())
        {
            m_instance.ToggleSaveResetButtons(true);
        }
        else
        {
            m_instance.ToggleSaveResetButtons(false);
        }

        // Modify the number of incrementations applied for this particular bonus.
        m_instance.m_bonuses[(int)bonus].Add(change);

        // Update in-memory values.
        m_instance.m_currentCredits -= change;
        m_instance.m_spentCredits += change;

        // Update the credit display.
        m_instance.UpdateCreditDisplay();

        // Update the bonus window.
        UpdateBonusDisplay(bonus, FindObjectOfType<PlayerPathFindingObject>());

        // Update all tickers.
        if (m_instance.m_currentCredits == 0)
        {
            // If there are no credits to add, hide all the add buttons.

            foreach (var field in m_instance.m_percentBonusField)
            {
                field.HideButton(false);
            }
        }
        else if (m_instance.m_spentCredits == 0)
        {
            // If there are no spent credits available, hide all the remove buttons.

            foreach (var field in m_instance.m_percentBonusField)
            {
                field.HideButton(true);
            }
        }
        else
        {
            // Else update all the buttons with the appropriate states according to the state of each bonus type.

            for (int i = 0; i < m_instance.m_percentBonusField.Length; i++)
            {
                m_instance.m_percentBonusField[i].SetButtonState(m_instance.m_bonuses[i].State());
            }
        }
    }

    /// <summary>
    /// Toggle the save and reset buttons - true will show both and vice versa.
    /// </summary>
    void ToggleSaveResetButtons(bool on)
    {
        foreach (var button in m_saveResetButtons)
        {
            button.SetActive(on);
        }
    }

    /// <summary>
    /// Returns a float representing the value of a stat after being modified by it's corresponding bonus, as a decimal representation
    /// of a percentage. For example, 15% will 0.15.
    /// </summary>
    public static float GetModifiedValueFlatAsDecimal(Enums.PLAYER_STAT bonus, float rawValue)
    {
        Debug.Log(m_instance.m_bonuses[(int)bonus].GetDecimal(rawValue));
        return m_instance.m_bonuses[(int)bonus].GetDecimal(rawValue);
    }

    /// <summary>
    /// Returns a float representing the value of a stat after being modified by it's corresponding bonus, as its real value after modification.
    /// </summary>
    public static float GetModifiedValue(Enums.PLAYER_STAT bonus, float rawValue, bool next = false)
    {
        if (next) return m_instance.m_bonuses[(int)bonus].GetNext(rawValue);
        return m_instance.m_bonuses[(int)bonus].Get(rawValue);
    }

    /// <summary>
    /// Returns the state of the bonus.
    /// </summary>
    public static Enums.BONUS_STATE GetBonusState(Enums.PLAYER_STAT bonus)
    {
        return m_instance.m_bonuses[(int)bonus].State();
    }
    
    /// <summary>
    /// Saves all bonus data to persistent data while resetting the state of this class. Also updates player to reflect these changes.
    /// </summary>
    public void Save()
    {
        // Note - this is done linearly, to prevent changes in the corresponding enums from ruining a for loop (for example).

        PersistentData.SaveInt(PersistentData.KEY_INT.BONUS_CRIT_CHANCE, m_bonuses[(int)Enums.PLAYER_STAT.CHANCE_CRIT].Increments);
        PersistentData.SaveInt(PersistentData.KEY_INT.BONUS_ATTACK_DAMAGE, m_bonuses[(int)Enums.PLAYER_STAT.BOOST_DAMAGE].Increments);
        PersistentData.SaveInt(PersistentData.KEY_INT.BONUS_ULT_DUR, m_bonuses[(int)Enums.PLAYER_STAT.DURATION_INCREASE_ULTIMATE].Increments);
        PersistentData.SaveInt(PersistentData.KEY_INT.BONUS_KICK_CD, m_bonuses[(int)Enums.PLAYER_STAT.COOLDOWN_KICK].Increments);
        PersistentData.SaveInt(PersistentData.KEY_INT.BONUS_SPIN_CD, m_bonuses[(int)Enums.PLAYER_STAT.COOLDOWN_SPIN].Increments);
        PersistentData.SaveInt(PersistentData.KEY_INT.BONUS_BASH_CD, m_bonuses[(int)Enums.PLAYER_STAT.COOLDOWN_SHIELD_BASH].Increments);
        PersistentData.SaveInt(PersistentData.KEY_INT.BONUS_REFLECT_CD, m_bonuses[(int)Enums.PLAYER_STAT.COOLDOWN_REFLECT].Increments);
        PersistentData.SaveInt(PersistentData.KEY_INT.BONUS_ULT_CD, m_bonuses[(int)Enums.PLAYER_STAT.COOLDOWN_ULT].Increments);
        PersistentData.SaveInt(PersistentData.KEY_INT.BONUS_HEALTH, m_bonuses[(int)Enums.PLAYER_STAT.BOOST_HEALTH].Increments);
        PersistentData.SaveInt(PersistentData.KEY_INT.BONUS_DODGE_CHANCE, m_bonuses[(int)Enums.PLAYER_STAT.CHANCE_DODGE].Increments);

        PersistentData.SaveInt(PersistentData.KEY_INT.CURRENT_CREDITS, m_currentCredits);
        PersistentData.SaveInt(PersistentData.KEY_INT.SPENT_CREDITS, m_spentCredits);

        FindObjectOfType<PlayerPathFindingObject>().UpdateHealthCap();

        ToggleSaveResetButtons(false);
        m_changeVector = new short[10];
    }

    /// <summary>
    /// Resets this class to previous known values.
    /// </summary>
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

    /// <summary>
    /// Returns an integer value that represents the attack bonus, after being modified by the current attack bonus.
    /// </summary>
    public static int GetAttackBonus(Enums.PLAYER_STAT bonus, bool next = false)
    {
        if (next) return (m_instance.m_bonuses[(int)bonus] as PercentBonus).GetIncreaseFactor(true);
        return (m_instance.m_bonuses[(int)bonus] as PercentBonus).GetIncreaseFactor(false);
    }
}
