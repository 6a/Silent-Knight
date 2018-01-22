using UnityEngine;

/// <summary>
/// Component for bonus menu stat adjuster. Handles adding/substracting and locking behaviours.
/// </summary>
public class StatAdjuster : MonoBehaviour
{
    // References to the negative and positive button GameObjects.
    [SerializeField] GameObject m_buttonNegative, m_buttonPositive;

    // Type of stat that this button represents.
    [SerializeField] Enums.PLAYER_STAT m_type;

    // State of each button.
    Enums.BUTTON_STATE m_neg, m_pos;

    // Delay between pressing the button, and when the ticker will start to auto-increment/decrement.
    const float DELAY = 0.5f;

    // Time that the button was pressed. Only one is required as it should only be possible to press one 
    // button at a time.
    float m_buttonDownTime = 0;

    void Awake()
    {
        m_neg = Enums.BUTTON_STATE.UP;
        m_pos = Enums.BUTTON_STATE.UP;
    }

    void Update()
    {
        // Every frame, button states and timers are inspected to see if the ticker should be automatically updating the bonus value.
        // For example, after a ticker is pressed and held down, after the set delay the bonus will start to tick up even without
        // multiple presses.
        
        if (m_neg == Enums.BUTTON_STATE.DOWN && Time.frameCount % 3 == 0 && Time.realtimeSinceStartup - m_buttonDownTime > DELAY)
        {
            RemovePointProtected(m_type);
        }

        if (m_pos == Enums.BUTTON_STATE.DOWN && Time.frameCount % 3 == 0 && Time.realtimeSinceStartup - m_buttonDownTime > DELAY)
        {
            AddPointProtected(m_type);
        }
    }

    /// <summary>
    /// Hides one of the ticker buttons for this bonus.
    /// </summary>
    public void HideButton(Enums.BUTTON_TYPE t)
    {
        switch (t)
        {
            case Enums.BUTTON_TYPE.POSITIVE:
                m_buttonPositive.SetActive(false);
                break;
            case Enums.BUTTON_TYPE.NEGATIVE:
                m_buttonNegative.SetActive(false);
                break;
        }
    }

    /// <summary>
    /// Updates the current ticker buttons according to a bonus state.
    /// </summary>
    public void SetState(Enums.BONUS_STATE state)
    {
        switch (state)
        {
            case Enums.BONUS_STATE.AT_MINIMUM:
                m_buttonNegative.SetActive(false);
                m_buttonPositive.SetActive(true);
                break;
            case Enums.BONUS_STATE.VALID:
                m_buttonNegative.SetActive(true);
                m_buttonPositive.SetActive(true);
                break;
            case Enums.BONUS_STATE.AT_MAXIMUM:
                m_buttonNegative.SetActive(true);
                m_buttonPositive.SetActive(false);
                break;
            case Enums.BONUS_STATE.INVALID:
                m_buttonNegative.SetActive(false);
                m_buttonPositive.SetActive(false);
                break;
        }
    }

    /// <summary>
    /// Adds a point to the appropriate bonus, and also protects against overflow.
    /// </summary>
    void AddPointProtected(Enums.PLAYER_STAT bonusType)
    {
        if (BonusManager.CanAdd(bonusType))
        {
            BonusManager.AddBonusAmount(bonusType, 1);
        }
        else
        {
            m_pos = Enums.BUTTON_STATE.UP;
        }
    }

    /// <summary>
    /// Removes a point from the appropriate bonus, and also protects against overflow.
    /// </summary>
    void RemovePointProtected(Enums.PLAYER_STAT bonusType)
    {
        if (BonusManager.CanSubtract(bonusType))
        {
            BonusManager.AddBonusAmount(bonusType, -1);
        }
        else
        {
            m_neg = Enums.BUTTON_STATE.UP;
        }
    }

    /// <summary>
    /// Button event helper, for when a point is removed (by clicking the negative button).
    /// </summary>
    public void OnRemoveDown(int bonusType)
    {
        m_neg = Enums.BUTTON_STATE.DOWN;
        m_buttonDownTime = Time.realtimeSinceStartup;
        RemovePointProtected(m_type);
    }

    /// <summary>
    /// Button event helper, for when the remove button is unpressed.
    /// </summary>
    public void OnRemoveUp()
    {
        m_neg = Enums.BUTTON_STATE.UP;
    }

    /// <summary>
    /// Button event helper, for when a point is added (by clicking the positive button).
    /// </summary>
    public void OnAddDown(int bonusType)
    {
        m_pos = Enums.BUTTON_STATE.DOWN;
        m_buttonDownTime = Time.realtimeSinceStartup;

        AddPointProtected(m_type);
    }

    /// <summary>
    /// Button event helper, for when the add button is unpressed.
    /// </summary>
    public void OnAddUp()
    {
        m_pos = Enums.BUTTON_STATE.UP;
    }
}
