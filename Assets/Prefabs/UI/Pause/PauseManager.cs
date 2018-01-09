using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [SerializeField] GameObject m_pauseScreen, m_gameScreen, m_bonusScreen, m_settingsScreen, m_resetScreen;

    bool m_paused;

    static PauseManager m_instance;

    void Awake()
    {
        m_instance = this;
    }

    public static bool Paused()
    {
        return m_instance.m_paused;
    }

    // 0 = pausemenu, 1 = bonusmenu, 2 = settings
    public void OnStartPause(int nextScreen)
    {
        m_gameScreen.SetActive(false);
        FindObjectOfType<JPlayerUnit>().GetComponent<Animator>().enabled = false;
        AI.PauseUnits();

        switch (nextScreen)
        {
            case 0: m_pauseScreen.SetActive(true); break;
            case 1:
                m_bonusScreen.SetActive(true);
                FindObjectOfType<JPlayerUnit>().UpdateBonusDisplay();
                break;
            case 2:
                m_pauseScreen.SetActive(false);
                m_settingsScreen.SetActive(true);
                return;
            case 3:
                m_resetScreen.SetActive(true);
                break;
            default: break;
        }
        Time.timeScale = 0;
        Camera.main.gameObject.GetComponent<UnityStandardAssets.ImageEffects.Blur>().enabled = true;
        m_paused = true;
    }


    public void OnCloseSettings()
    {
        m_settingsScreen.SetActive(false);
        m_pauseScreen.SetActive(true);

        FindObjectOfType<JPlayerUnit>().GetComponent<Animator>().enabled = true;
    }

    // 0 = pausemenu, 1 = bonusmenu
    public void OnEndPause()
    {
        m_pauseScreen.SetActive(false);
        m_bonusScreen.SetActive(false);
        m_resetScreen.SetActive(false);

        m_gameScreen.SetActive(true);

        Time.timeScale = 1;
        Camera.main.gameObject.GetComponent<UnityStandardAssets.ImageEffects.Blur>().enabled = false;
        FindObjectOfType<JPlayerUnit>().GetComponent<Animator>().enabled = true;
        AI.UnPauseUnits();
        m_paused = false;
    }
}
