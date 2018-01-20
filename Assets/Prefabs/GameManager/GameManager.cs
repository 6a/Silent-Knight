using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Delegate for level start sequencing.
/// </summary>
public delegate void LevelLoadTrigger();

/// <summary>
/// Provices an interface for multiple core game processes, as well as various UI functions.
/// </summary>
public class GameManager : MonoBehaviour
{
    // In class reference to a level load trigger event for registration.
    public static event LevelLoadTrigger OnStartRun;

    // Reference to the dungeon generator helper.
    DungeonGenerator m_generator;

    // References to in-game UI objects
    [SerializeField] GameObject m_loadingBlockTopParent, m_loadingBlockBottomParent;
    [SerializeField] Image m_faderBackground, m_faderLogo;
    [SerializeField] GameObject m_endOptions;
    [SerializeField] GameObject m_deathOptions;
    Image [] m_loadingBlocksBottom, m_loadingBlocksTop;

    // Delay to add to each loading sequence. 
    [SerializeField] float m_loadDelay;

    // reference to the player unit.
    PlayerPathFindingObject m_currentPlayer;

    // Whether or not the current level loading process should continue (temporary state holder).
    bool m_continueLevelLoad;

    static GameManager m_instance;

    void Awake()
    {
        m_loadingBlocksBottom = m_loadingBlockBottomParent.GetComponentsInChildren<Image>(true);
        m_loadingBlocksTop = m_loadingBlockTopParent.GetComponentsInChildren<Image>(true);

        m_generator = FindObjectOfType<DungeonGenerator>();
        m_instance = this;
    }

    void Start ()
    {
        LoadNext();
    }

    /// <summary>
    /// Registers the current player.
    /// </summary>
    public static void RegisterPlayer(PlayerPathFindingObject reference) { m_instance.m_currentPlayer = reference; }

    /// <summary>
    /// Initiates a fade to black using the UI.
    /// </summary>
    public static void FadeToBlack(bool death)
    {
        m_instance.StartCoroutine(m_instance.FadeToBlackAsync(death));
    }

    /// <summary>
    /// Asynchronous component of FadeToBlack().
    /// </summary>
    IEnumerator FadeToBlackAsync(bool death)
    {
        float increment = 0.05f;

        // Fades all fader UI objects
        while (m_faderBackground.color.a < 1)
        {
            var fc = m_faderBackground.color;
            m_faderBackground.color = new Color(fc.r, fc.g, fc.b, fc.a + increment);

            yield return new WaitForSecondsRealtime(0.05f);
        }

        m_deathOptions.SetActive(death);
    }

    /// <summary>
    /// Enables (shows) the loading screen.
    /// </summary>
    public static void EnableLoadingScreen()
    {
        // All objects alpha channels are reset to 1.

        var fc = m_instance.m_faderBackground.color;
        m_instance.m_faderBackground.color = new Color(fc.r, fc.g, fc.b, 1);

        var lc = m_instance.m_faderLogo.color;
        m_instance.m_faderLogo.color = new Color(lc.r, lc.g, lc.b, 1);

        for (int i = 0; i < m_instance.m_loadingBlocksBottom.Length; i++)
        {
            var bc = m_instance.m_loadingBlocksBottom[i].color;
            m_instance.m_loadingBlocksBottom[i].color = new Color(bc.r, bc.g, bc.b, 1);

            var tc = m_instance.m_loadingBlocksTop[i].color;
            m_instance.m_loadingBlocksTop[i].color = new Color(tc.r, tc.g, tc.b, 1);
        }
    }

    /// <summary>
    /// Hides the loading screen.
    /// </summary>
    public static void DisableLoadingScreen()
    {
        // All objects alpha channels are reset to 1.

        var fc = m_instance.m_faderBackground.color;
        m_instance.m_faderBackground.color = new Color(fc.r, fc.g, fc.b, 0);

        var lc = m_instance.m_faderLogo.color;
        m_instance.m_faderLogo.color = new Color(lc.r, lc.g, lc.b, 0);

        for (int i = 0; i < m_instance.m_loadingBlocksBottom.Length; i++)
        {
            var bc = m_instance.m_loadingBlocksBottom[i].color;
            m_instance.m_loadingBlocksBottom[i].color = new Color(bc.r, bc.g, bc.b, 0);

            var tc = m_instance.m_loadingBlocksTop[i].color;
            m_instance.m_loadingBlocksTop[i].color = new Color(tc.r, tc.g, tc.b, 0);
        }
    }

    /// <summary>
    /// Triggers a new level load sequence.
    /// </summary>
    void LoadNext()
    {
        StartCoroutine(LevelStart());
    }

    /// <summary>
    /// Called by the shatter class when it is ready for the load to progress.
    /// </summary>
    public static void ContinueLevelStart()
    {
        m_instance.m_continueLevelLoad = true;
    }

    /// <summary>
    /// Asynchronously starts a new level load.
    /// </summary>
    IEnumerator LevelStart(bool shatter = false)
    {
        // Update internal state.
        m_continueLevelLoad = false;

        // Execute shatter, if appropriate.
        if (shatter)
        {
            Shatter.StartShatter();
        }
        else
        {
            m_continueLevelLoad = true;
        }

        // If shattering, wait for shatter class to be ready to continue.
        while (!m_continueLevelLoad)
        {
            yield return null;
        }

        // Begind asynchronous load operation in DungeonGenerator.
        m_generator.IsLoadingAsync = true;
        StartCoroutine(m_generator.LoadNextAsync());

        // Wait until ready, then update the progress bar, and perform the shatter effect if required.
        int index = 10;
        while (m_generator.IsLoadingAsync || index < (10 + m_loadDelay / 0.1f) || (!Shatter.ShatterFinished && shatter))
        {
            if (!m_generator.IsLoadingAsync)
            {   
                if (shatter)
                {
                    Shatter.CompleteShatter();
                    GameUIManager.Reset();
                }
            }

            yield return new WaitForSecondsRealtime(0.1f);

            int indexCapped = index % (m_loadingBlocksTop.Length);

            m_loadingBlocksTop[indexCapped].enabled = true;

            indexCapped = (index - 1) % (m_loadingBlocksTop.Length);

            m_loadingBlocksTop[indexCapped].enabled = false;

            index++;
        }

        // Loading is complete, start fading the screen back in.
        float increment = 0.05f;
        while (m_faderBackground.color.a > 0)
        {
            var fc = m_faderBackground.color;
            m_faderBackground.color = new Color(fc.r, fc.g, fc.b, fc.a - increment);

            var lc = m_faderLogo.color;
            m_faderLogo.color = new Color(lc.r, lc.g, lc.b, lc.a - increment);


            for (int i = 0; i < m_loadingBlocksBottom.Length; i++)
            {
                var bc = m_loadingBlocksBottom[i].color;
                m_loadingBlocksBottom[i].color = new Color(bc.r, bc.g, bc.b, bc.a - increment);

                var tc = m_loadingBlocksTop[i].color;
                m_loadingBlocksTop[i].color = new Color(tc.r, tc.g, tc.b, tc.a - increment);
            }

            yield return new WaitForSecondsRealtime(0.05f);
        }


        // Once the screen has been fully faded in, start the rest of the loading process and inform all registered units.
        DisableLoadingScreen();
        OnStartRun();
    }

    /// <summary>
    /// Returns a reference to the current player unit.
    /// </summary>
    public static PlayerPathFindingObject GetCurrentPlayerReference()
    {
        return m_instance.m_currentPlayer;
    }

    /// <summary>
    /// Trigger a new level load event.
    /// </summary>
    public static void TriggerLevelLoad()
    {
        OnStartRun = null;

        m_instance.m_generator.NextLevelSetup();

        m_instance.StartCoroutine(m_instance.LevelStart(true));
    }

    /// <summary>
    /// Enable the end screen.
    /// </summary>
    public static void TriggerEndScreen()
    {
        OnStartRun = null;
        m_instance.StartCoroutine(m_instance.EndScreen());
    }

    /// <summary>
    /// Asynchronous component of end screen init.
    /// </summary>
    IEnumerator EndScreen()
    {
        Shatter.StartShatter();

        yield return new WaitForEndOfFrame();

        m_endOptions.SetActive(true);

        for (int i = 0; i < m_loadingBlocksBottom.Length; i++)
        {
            m_loadingBlocksBottom[i].enabled = false;
            m_loadingBlocksTop[i].enabled = false;
        }

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        Shatter.CompleteShatter();

        while (!Shatter.ShatterFinished)
        {
            yield return null;
        }
    }

    /// <summary>
    /// Wipes all saved settings and reloads the game.
    /// </summary>
    public static void TotalReset()
    {
        OnStartRun = null;

        var intsToReset = Enum.GetNames(typeof(PersistentData.KEY_INT)).Length;

        var excludes = new PersistentData.KEY_INT [] { PersistentData.KEY_INT.LANGUAGE, PersistentData.KEY_INT.LEVEL_GFX };

        for (int i = 0; i < intsToReset; i++)
        {
            if (Array.IndexOf(excludes, (PersistentData.KEY_INT)i) == -1)
            {
                PersistentData.SaveInt((PersistentData.KEY_INT)i, 0);
            }
        }

        Time.timeScale = 1;
        AudioManager.CrossFadeBGM(Enums.BGM_VARIATION.QUIET, 2);

        SceneManager.LoadScene(1);
    }

    /// <summary>
    /// Reincarnates the player (progress reset to level 1, dungeon 1. Skill points are not removed).
    /// </summary>
    public static void Reincarnate()
    {
        OnStartRun = null;

        PersistentData.SaveInt(PersistentData.KEY_INT.XP, 0);
        PersistentData.SaveInt(PersistentData.KEY_INT.LEVEL, 0);

        Time.timeScale = 1;
        AudioManager.CrossFadeBGM(Enums.BGM_VARIATION.QUIET, 2);
        SceneManager.LoadScene(1);
    }

    /// <summary>
    /// Reloads the level (simply reloads the scene).
    /// </summary>
    public static void ReloadLevel()
    {
        OnStartRun = null;

        Time.timeScale = 1;
        AudioManager.CrossFadeBGM(Enums.BGM_VARIATION.QUIET, 2);
        SceneManager.LoadScene(1);
    }

    /// <summary>
    /// Public function for end of game button, for reincarnation, via a GUI event.
    /// </summary>
    public void ReincarnateEnd()
    {
        Reincarnate();
    }

    /// <summary>
    /// Public function for quitting the game via a GUI event.
    /// </summary>
    public void Quit()
    {
        Application.Quit();
    }
}
