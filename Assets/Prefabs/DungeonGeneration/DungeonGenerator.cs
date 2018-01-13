using DungeonGeneration;
using UnityEngine;
using PathFinding;
using System.Collections;
using System.Diagnostics;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] int m_maxDungeonWidth;
    [SerializeField] int m_maxDungeonHeight;
    [SerializeField] int m_minWidth, m_maxWidth;
    [SerializeField] int m_minHeight, m_maxHeight;
    [SerializeField] int m_cycles;
    [SerializeField] int m_padding;
    [SerializeField] int m_minPlatforms;
    [SerializeField] char m_emptyChar;
    [SerializeField] char m_platformChar;
    [SerializeField] char m_nodeChar;
    [SerializeField] char m_pathChar;
    [SerializeField] int m_scale;
    [SerializeField] int m_offset;
    [SerializeField] int [] m_levelSeeds;

    [SerializeField] int m_iterations;

    [SerializeField] GameObject m_previewPlane;

    int m_currentLevel;

    ASGrid m_grid;

    void Awake ()
    {
        m_grid = FindObjectOfType<ASGrid>();

        m_currentLevel = PPM.LoadInt(PPM.KEY_INT.LEVEL);

        Generator.Init(m_maxDungeonWidth, m_maxDungeonHeight,
        new PlatformProperties(m_minWidth, m_maxWidth, m_minHeight, m_maxHeight),
        m_cycles, m_padding, m_minPlatforms, m_emptyChar, m_platformChar, m_nodeChar, m_pathChar, m_scale, m_offset, m_currentLevel);
    }

    public void DiscoverValidLevels(int iterations, int start, bool fabricate = false, int platforms = 10, int nodes = 2, float wait = 0)
    {
        m_currentLevel = start;

        StartCoroutine(TestLevels(iterations, start, fabricate, platforms, nodes, wait));
    }

    IEnumerator TestLevels(int iterations, int start, bool fabricate, int platforms, int nodes, float wait)
    {
        var validLevels = new System.Collections.Generic.List<int>();

        for (int i = start; i < iterations + start; i++)
        {
            UpdateTestDungeon(i);

            if (i > 0 && Generator.CurrentDungeon.Nodes.Count == nodes && Generator.CurrentDungeon.Platforms.Count == platforms)
            {
                UnityEngine.Debug.Log("Suitable dungeon: " + i);
                FabricateTest(i);
                validLevels.Add(i);
            }

            yield return new WaitForSeconds(wait);
        }

        UnityEngine.Debug.ClearDeveloperConsole();
        print("SEARCH COMPLETE ------------------------------------");
        print("SUITABLE LEVELS ------------------------------------");

        foreach (var lvl in validLevels)
        {
            print("Level: " + lvl);
        }
    }

    public bool IsFinalLevel()
    {
        return m_currentLevel == m_levelSeeds.Length - 1;
    }
    
    public void NextLevelSetup()
    {
        m_currentLevel++;
        PPM.SaveInt(PPM.KEY_INT.LEVEL, m_currentLevel);
        Generator.InitNewLevel(m_currentLevel);
    }

    public bool IsLoadingAsync { get; set; }

    public IEnumerator LoadNextAsync()
    {

        Stopwatch s = new Stopwatch();
        s.Start();
        UpdateDungeon(m_levelSeeds[m_currentLevel]);

        UpdatePreviewTexture();

        Generator.Fabricate();

        yield return new WaitForFixedUpdate();

        m_grid.CreateGrid();
        s.Stop();

        IsLoadingAsync = false;
        yield return null;
    }

    public void LoadNext()
    {
        for (int i = 0; i < m_iterations; i++)
        {
            if (i > 0) m_levelSeeds[0]++;

            UpdateDungeon(m_levelSeeds[m_currentLevel]);
            UpdatePreviewTexture();

            if (i > 0 && Generator.CurrentDungeon.Nodes.Count == 2 && Generator.CurrentDungeon.Platforms.Count == 10)
            {
                UnityEngine.Debug.Log("Suitable dungeon: " + m_levelSeeds[m_currentLevel]);
            }
        }

        Generator.Fabricate();
        m_grid.CreateGrid();
    }

    void UpdateTestDungeon(int seed)
    {
        Generator.GenerateTestDungeon(seed);
    }

    void FabricateTest(int level)
    {
        Generator.FabricateTest(level);
    }

    void UpdateDungeon(int seed)
    {
        Generator.GenerateNewDungeon(seed);
    }
    
    void UpdatePreviewTexture()
    {
        var t = Generator.CurrentDungeon.Texture;
        m_previewPlane.GetComponent<MeshRenderer>().material.mainTexture = t;
    }
}
