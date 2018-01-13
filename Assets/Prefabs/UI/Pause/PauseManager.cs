using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    enum STATE { PAUSE, GAME, BONUS, SETTINGS, RESET }

    [SerializeField] GameObject m_pauseScreen, m_gameScreen, m_bonusScreen, m_settingsScreen, m_resetScreen;

    bool m_paused;
    Stack<STATE> m_state;

    static PauseManager m_instance;

    void Awake()
    {
        m_instance = this;
        m_state = new Stack<STATE>();
        m_state.Push(STATE.GAME);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Back();
        }
    }

    void Back()
    {
        if (m_state.Count == 1)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            activity.Call<bool>("moveTaskToBack", true);
#endif
            return;
        }

        var current = m_state.Pop();

        var state = m_state.Peek();

        m_state.Push(current);

        SetUI(state, true);
    }

    void SetUI(STATE s, bool back)
    {
        m_gameScreen.SetActive(false);
        m_pauseScreen.SetActive(false);
        m_bonusScreen.SetActive(false);
        m_resetScreen.SetActive(false);
        m_settingsScreen.SetActive(false);

        switch (s)
        {
            case STATE.PAUSE:
                m_pauseScreen.SetActive(true); 

                Time.timeScale = 0;
                Camera.main.gameObject.GetComponent<UnityStandardAssets.ImageEffects.Blur>().enabled = true;
                m_paused = true;

                if (!back) m_state.Push(STATE.PAUSE);
                else m_state.Pop();
                break;
            case STATE.GAME:
                m_gameScreen.SetActive(true);

                Time.timeScale = 1;
                Camera.main.gameObject.GetComponent<UnityStandardAssets.ImageEffects.Blur>().enabled = false;
                FindObjectOfType<JPlayerUnit>().GetComponent<Animator>().enabled = true;
                AI.UnPauseUnits();
                m_paused = false;

                m_state.Pop();
                break;
            case STATE.BONUS:
                m_bonusScreen.SetActive(true);
                FindObjectOfType<JPlayerUnit>().UpdateBonusDisplay();

                Time.timeScale = 0;
                Camera.main.gameObject.GetComponent<UnityStandardAssets.ImageEffects.Blur>().enabled = true;
                m_paused = true;

                if (!back) m_state.Push(STATE.BONUS);
                else m_state.Pop();
                break;
            case STATE.SETTINGS:
                m_settingsScreen.SetActive(true);

                Time.timeScale = 0;
                Camera.main.gameObject.GetComponent<UnityStandardAssets.ImageEffects.Blur>().enabled = true;
                m_paused = true;

                if (!back) m_state.Push(STATE.SETTINGS);
                else m_state.Pop();
                break;
            case STATE.RESET:
                m_resetScreen.SetActive(true);

                Time.timeScale = 0;
                Camera.main.gameObject.GetComponent<UnityStandardAssets.ImageEffects.Blur>().enabled = true;
                m_paused = true;

                if (!back) m_state.Push(STATE.RESET);
                else m_state.Pop();
                break;
        }
    }

    public static bool Paused()
    {
        return m_instance.m_paused;
    }

    // 0 = pausemenu, 1 = bonusmenu, 2 = settings, 3 = reset
    public void OnStartPause(int nextScreen)
    {
        FindObjectOfType<JPlayerUnit>().GetComponent<Animator>().enabled = false;
        AI.PauseUnits();

        switch (nextScreen)
        {
            case 0: SetUI(STATE.PAUSE, false); break;
            case 1: SetUI(STATE.BONUS, false); break;
            case 2: SetUI(STATE.SETTINGS, false); break;
            case 3: SetUI(STATE.RESET, false); break;
        }
    }


    public void OnCloseSettings()
    {
        Back();
    }

    public void OnEndPause()
    {
        SetUI(STATE.GAME, true);
    }
}
