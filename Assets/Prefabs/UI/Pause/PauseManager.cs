using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    // References to the GameOnbjects containing the various UI screens.
    [SerializeField] GameObject m_pauseScreen, m_gameScreen, m_bonusScreen, m_settingsScreen, m_resetScreen;

    // State variable for determining if the game is running or not (paused).
    bool m_paused;

    // Stack containing the state history. Used for navigating backwards through screens in order.
    Stack<Enums.UI_STATE> m_state;

    static PauseManager m_instance;

    void Awake()
    {
        m_instance = this;
        m_state = new Stack<Enums.UI_STATE>();
        m_state.Push(Enums.UI_STATE.GAME);
    }

    void Update()
    {
        // Detect the Escape keycode. On PC, this is the esc. key. On Android, this is the back button.
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Back();
        }
    }

    /// <summary>
    /// Pop the most recent state off of the stack and revert to the previous state.
    /// </summary>
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

    /// <summary>
    /// Set the UI to a particular state. If back, this is an operation moving backwards from one state, back to a previous one.
    /// </summary>
    /// Note: Each state has a specific set of actions to perform, as well as managing the internal state stack depending
    /// On whether this is a forwards or backwards operation.
    void SetUI(Enums.UI_STATE state, bool back)
    {
        m_gameScreen.SetActive(false);
        m_pauseScreen.SetActive(false);
        m_bonusScreen.SetActive(false);
        m_resetScreen.SetActive(false);
        m_settingsScreen.SetActive(false);

        switch (state)
        {
            case Enums.UI_STATE.PAUSE:
                m_pauseScreen.SetActive(true); 

                Time.timeScale = 0;
                Camera.main.gameObject.GetComponent<UnityStandardAssets.ImageEffects.Blur>().enabled = true;
                m_paused = true;

                if (!back) m_state.Push(Enums.UI_STATE.PAUSE);
                else m_state.Pop();

                break;
            case Enums.UI_STATE.GAME:
                m_gameScreen.SetActive(true);

                Time.timeScale = 1;
                Camera.main.gameObject.GetComponent<UnityStandardAssets.ImageEffects.Blur>().enabled = false;
                FindObjectOfType<PlayerPathFindingObject>().GetComponent<Animator>().enabled = true;
                AIManager.UnPauseUnits();
                m_paused = false;

                m_state.Clear();
                m_state.Push(Enums.UI_STATE.GAME);

                break;
            case Enums.UI_STATE.BONUS:
                m_bonusScreen.SetActive(true);
                FindObjectOfType<PlayerPathFindingObject>().UpdateBonusDisplay();

                Time.timeScale = 0;
                Camera.main.gameObject.GetComponent<UnityStandardAssets.ImageEffects.Blur>().enabled = true;
                m_paused = true;

                if (!back) m_state.Push(Enums.UI_STATE.BONUS);
                else m_state.Pop();

                break;
            case Enums.UI_STATE.SETTINGS:
                m_settingsScreen.SetActive(true);

                Time.timeScale = 0;
                Camera.main.gameObject.GetComponent<UnityStandardAssets.ImageEffects.Blur>().enabled = true;
                m_paused = true;

                if (!back) m_state.Push(Enums.UI_STATE.SETTINGS);
                else m_state.Pop();

                break;
            case Enums.UI_STATE.RESET:
                m_resetScreen.SetActive(true);

                Time.timeScale = 0;
                Camera.main.gameObject.GetComponent<UnityStandardAssets.ImageEffects.Blur>().enabled = true;
                m_paused = true;

                if (!back) m_state.Push(Enums.UI_STATE.RESET);
                else m_state.Pop();

                break;
        }
    }

    /// <summary>
    /// Returns true if the game is currently paused (in a menu etc.).
    /// </summary>
    /// <returns></returns>
    public static bool IsPaused()
    {
        return m_instance.m_paused;
    }

    ///<summary>
    /// Button function. Pauses the game and opens up the requested ui screen. int is cast to UI_STATE enum.
    /// </summary>
    /// Note: 0 = pausemenu, 1 = bonusmenu, 2 = settings, 3 = reset
    public void OnStartPause(int nextScreen)
    {
        FindObjectOfType<PlayerPathFindingObject>().GetComponent<Animator>().enabled = false;
        AIManager.PauseUnits();

        switch (nextScreen)
        {
            case 0: SetUI(Enums.UI_STATE.PAUSE, false); break;
            case 1: SetUI(Enums.UI_STATE.BONUS, false); break;
            case 2: SetUI(Enums.UI_STATE.SETTINGS, false); break;
            case 3: SetUI(Enums.UI_STATE.RESET, false); break;
        }
    }

    /// <summary>
    /// Button function - specifically for the settings screen being closed.
    /// </summary>
    public void OnCloseSettings()
    {
        Back();
    }

    /// <summary>
    /// Skip past any history and move straight back to the game.
    /// </summary>
    public void OnEndPause()
    {
        SetUI(Enums.UI_STATE.GAME, true);
    }
}
