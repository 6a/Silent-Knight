using UnityEngine;
// TODO start from here.
public class StatAdjuster : MonoBehaviour
{
    enum BUTTON_STATE { UP, DOWN }

    [SerializeField] GameObject m_buttonNegative, m_buttonPositive;

    BUTTON_STATE m_neg, m_pos;
    Enums.PLAYER_STAT m_type;

    const float DELAY = 0.5f;
    float m_buttonDownTime = 0;

    void Awake()
    {
        m_neg = BUTTON_STATE.UP;
        m_pos = BUTTON_STATE.UP;
    }

    public void HideButton(bool down)
    {
        if (down)
        {
            m_buttonNegative.SetActive(false);
        }
        else
        {
            m_buttonPositive.SetActive(false);
        }
    }

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

    void OnAddPoint(Enums.PLAYER_STAT bonusType)
    {
        if (BonusManager.CanAdd(bonusType))
        {
            BonusManager.AddBonusAmount(bonusType, 1);
        }
        else
        {
            m_pos = BUTTON_STATE.UP;
        }
    }

    void OnRemovePoint(Enums.PLAYER_STAT bonusType)
    {
        if (BonusManager.CanSubtract(bonusType))
        {
            BonusManager.AddBonusAmount(bonusType, -1);
        }
        else
        {
            m_neg = BUTTON_STATE.UP;
        }
    }
    
    public void OnRemoveDown(int bonusType)
    {
        m_type = (Enums.PLAYER_STAT)bonusType;
        m_neg = BUTTON_STATE.DOWN;
        m_buttonDownTime = Time.realtimeSinceStartup;
        OnRemovePoint(m_type);
    }

    public void OnRemoveUp()
    {
        m_neg = BUTTON_STATE.UP;
    }

    public void OnAddDown(int bonusType)
    {
        m_type = (Enums.PLAYER_STAT)bonusType;
        m_pos = BUTTON_STATE.DOWN;
        m_buttonDownTime = Time.realtimeSinceStartup;

        OnAddPoint(m_type);
    }

    public void OnAddUp()
    {
        m_pos = BUTTON_STATE.UP;
    }

    private void Update()
    {
        if (m_neg == BUTTON_STATE.DOWN && Time.frameCount % 3 == 0 && Time.realtimeSinceStartup - m_buttonDownTime > DELAY)
        {
            OnRemovePoint(m_type);
        }

        if (m_pos == BUTTON_STATE.DOWN && Time.frameCount % 3 == 0 && Time.realtimeSinceStartup - m_buttonDownTime > DELAY)
        {
            OnAddPoint(m_type);
        }

    }

}
