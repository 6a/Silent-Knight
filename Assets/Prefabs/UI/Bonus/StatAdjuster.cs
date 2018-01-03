using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class StatAdjuster : MonoBehaviour
{
    [SerializeField] GameObject m_buttonNegative, m_buttonPositive;

	void Start ()
    {
		
	}

	void Update ()
    {
		
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

    public void SetState(BONUS_STATE state)
    {
        switch (state)
        {
            case BONUS_STATE.AT_MINIMUM:
                m_buttonNegative.SetActive(false);
                m_buttonPositive.SetActive(true);
                break;
            case BONUS_STATE.VALID:
                m_buttonNegative.SetActive(true);
                m_buttonPositive.SetActive(true);
                break;
            case BONUS_STATE.AT_MAXIMUM:
                m_buttonNegative.SetActive(true);
                m_buttonPositive.SetActive(false);
                break;
            case BONUS_STATE.INVALID:
                m_buttonNegative.SetActive(false);
                m_buttonPositive.SetActive(false);
                break;
        }
    }

    // gets cast to BONUS enum
    public void OnAddPoint(int bonusType)
    {
        BonusManager.UpdateBonusAmount((BONUS)bonusType, 1);
    }

    // gets cast to BONUS enum
    public void OnRemovePoint(int bonusType)
    {
        BonusManager.UpdateBonusAmount((BONUS)bonusType, -1);
    }
}
