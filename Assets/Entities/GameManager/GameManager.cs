using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public delegate void LevelLoadTrigger();

public class GameManager : MonoBehaviour
{
    public static GAMESTATE GameState { get; set; }

    public static event LevelLoadTrigger OnStartRun;

    DungeonGenerator m_generator;

    [SerializeField] GameObject m_loadingBlockTopParent, m_loadingBlockBottomParent;
    [SerializeField] Image m_faderBackground, m_faderLogo;
    Image [] m_loadingBlocksBottom, m_loadingBlocksTop;

    [SerializeField] float m_loadDelay;
    [SerializeField] bool m_triggerShatter;

    static GameManager m_instance;

    void Awake()
    {
        m_loadingBlocksBottom = m_loadingBlockBottomParent.GetComponentsInChildren<Image>(true);
        m_loadingBlocksTop = m_loadingBlockTopParent.GetComponentsInChildren<Image>(true);

        m_generator = FindObjectOfType<DungeonGenerator>();
        OnStartRun += NextLevelSequence;
        GameState = GAMESTATE.START;
        m_instance = this;
    }

    void Start ()
    {
        // Startup sequence
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

    public void NextLevelSequence()
    {
        Debug.Log("GAME MANAGER: Next Level Trigger received");
    }

    void LoadNext()
    {
        StartCoroutine(LevelStart());
    }

    IEnumerator LevelStart ()
    {
        m_generator.IsLoadingAsync = true;
        StartCoroutine(m_generator.LoadNextAsync());

        int index = 10;

        while (m_generator.IsLoadingAsync || index < (10 + m_loadDelay / 0.1f))
        {
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

        OnStartRun();
    }
}
