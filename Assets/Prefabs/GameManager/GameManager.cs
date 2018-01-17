using System;
using System.Collections;
using Entities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public delegate void LevelLoadTrigger();

public class GameManager : MonoBehaviour
{
    public static GAMESTATE GameState { get; set; }

    public static event LevelLoadTrigger OnStartRun;

    DungeonGenerator m_generator;

    [SerializeField] GameObject m_loadingBlockTopParent, m_loadingBlockBottomParent;
    [SerializeField] Image m_faderBackground, m_faderLogo;
    [SerializeField] GameObject m_endOptions;
    [SerializeField] GameObject m_deathOptions;
    Image [] m_loadingBlocksBottom, m_loadingBlocksTop;

    [SerializeField] float m_loadDelay;
    [SerializeField] bool m_triggerShatter;
    [SerializeField] Texture2D m_cursor;

    PlayerPathFindingObject m_currentPlayer;

    static GameManager m_instance;

    void Awake()
    {
        m_loadingBlocksBottom = m_loadingBlockBottomParent.GetComponentsInChildren<Image>(true);
        m_loadingBlocksTop = m_loadingBlockTopParent.GetComponentsInChildren<Image>(true);

        m_generator = FindObjectOfType<DungeonGenerator>();
        OnStartRun += NextLevelSequence;
        GameState = GAMESTATE.START;
        m_instance = this;

#if UNITY_EDITOR
        Cursor.SetCursor(m_cursor, Vector2.zero, CursorMode.Auto);
#endif
    }

    void Start ()
    {
        GameState = GAMESTATE.GAMEPLAY;

        LoadNext();
    }

	void Update ()
    {
		if (m_triggerShatter)
        {
            m_triggerShatter = false;

            Shatter.StartShatter();
        }
	}

    public static void RegisterPlayer(PlayerPathFindingObject reference) { m_instance.m_currentPlayer = reference; }

    public static void FadeToBlack(bool death)
    {
        m_instance.StartCoroutine(m_instance.FadeToBlackAsync(death));
    }

    IEnumerator FadeToBlackAsync(bool death)
    {
        float increment = 0.05f;

        while (m_faderBackground.color.a < 1)
        {
            var fc = m_faderBackground.color;
            m_faderBackground.color = new Color(fc.r, fc.g, fc.b, fc.a + increment);

            yield return new WaitForSecondsRealtime(0.05f);
        }

        m_deathOptions.SetActive(death);
    }

    public static void EnableLoadingScreen()
    {
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

    public static void DisableLoadingScreen()
    {
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

    public void NextLevelSequence()
    {

    }

    void LoadNext()
    {
        StartCoroutine(LevelStart());
    }

    public static void ContinueLevelStart()
    {
        m_instance.m_continueLevelLoad = true;
    }

    bool m_continueLevelLoad;

    IEnumerator LevelStart(bool shatter = false)
    {
        m_continueLevelLoad = false;
        if (shatter)
        {
            Shatter.StartShatter();
        }
        else
        {
            m_continueLevelLoad = true;
        }

        while (!m_continueLevelLoad)
        {
            yield return null;
        }

        m_generator.IsLoadingAsync = true;
        StartCoroutine(m_generator.LoadNextAsync());

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

        DisableLoadingScreen();
        OnStartRun();
    }

    public static PlayerPathFindingObject GetCurrentPlayerReference()
    {
        return m_instance.m_currentPlayer;
    }

    public static void TriggerLevelLoad()
    {
        OnStartRun = null;

        m_instance.m_generator.NextLevelSetup();

        m_instance.StartCoroutine(m_instance.LevelStart(true));
    }

    public static void TriggerEndScreen()
    {
        OnStartRun = null;
        m_instance.StartCoroutine(m_instance.EndScreen());
    }

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

    // Just wipes all saved settings and reloads the game.
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
        Audio.BlendMusicTo(Audio.BGM.QUIET, 2);

        SceneManager.LoadScene(1);
    }

    public static void Reincarnate()
    {
        OnStartRun = null;

        PersistentData.SaveInt(PersistentData.KEY_INT.XP, 0);
        PersistentData.SaveInt(PersistentData.KEY_INT.LEVEL, 0);

        Time.timeScale = 1;
        Audio.BlendMusicTo(Audio.BGM.QUIET, 2);
        SceneManager.LoadScene(1);
    }
    
    public static void ReloadLevel()
    {
        OnStartRun = null;

        Time.timeScale = 1;
        Audio.BlendMusicTo(Audio.BGM.QUIET, 2);
        SceneManager.LoadScene(1);
    }

    public void ReincarnateEnd()
    {
        Reincarnate();
    }

    public void Quit()
    {
        Application.Quit();
    }

}
