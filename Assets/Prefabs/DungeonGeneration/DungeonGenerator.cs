using DungeonGeneration;
using UnityEngine;
using PathFinding;
using System.Collections;
using System.Diagnostics;

/// <summary>
/// Handles requests for dungeon generation and validation.
/// </summary>
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

    // Reference to the debug plane found that can be used to inspect generated dungeons as bitmap tilemaps.
    [SerializeField] GameObject m_previewPlane;

    // Used to lock activity during async loads.
    public bool IsLoadingAsync { get; set; }

    int m_currentLevel;

    // Reference to AStart grid class.
    ASGrid m_grid;

    void Awake ()
    {
        m_grid = FindObjectOfType<ASGrid>();

        m_currentLevel = PersistentData.LoadInt(PersistentData.KEY_INT.LEVEL);

        // This class will, by default, kick off the initialisation of a new dungeon.
        Generator.Init(m_maxDungeonWidth, m_maxDungeonHeight,
        new PlatformProperties(m_minWidth, m_maxWidth, m_minHeight, m_maxHeight),
        m_cycles, m_padding, m_minPlatforms, m_emptyChar, m_platformChar, m_nodeChar, m_pathChar, m_scale, m_offset, m_currentLevel);
    }

    /// <summary>
    /// Tester function for finding valid levels. WARNING: ONLY RUN IN DUNGEON TEST SCENE.
    /// </summary>
    public void DiscoverValidLevels(int iterations, int start, bool fabricate = false, int platforms = 10, int nodes = 2, float wait = 0)
    {
        m_currentLevel = start;

        StartCoroutine(DiscoverValidLevelsAsync(iterations, start, fabricate, platforms, nodes, wait));
    }

    IEnumerator DiscoverValidLevelsAsync(int iterations, int start, bool fabricate, int platforms, int nodes, float wait)
    {
        // Create a new, empty list of integers to store the valid level seeds.
        var validLevels = new System.Collections.Generic.List<int>();

        // Run through every seed to be tested. Generate the dungeon and add the seed to the list if valid.
        for (int i = start; i < iterations + start; i++)
        {
            Generator.GenerateTestDungeon(i);

            if (i > 0 && Generator.CurrentDungeon.Nodes.Count == nodes && Generator.CurrentDungeon.Platforms.Count == platforms)
            {
                UnityEngine.Debug.Log("Suitable dungeon: " + i);
                validLevels.Add(i);
            }

            yield return null;
        }

        UnityEngine.Debug.ClearDeveloperConsole();
        print("SEARCH COMPLETE ------------------------------------");
        print("SUITABLE LEVELS ------------------------------------");

        var nextT = 0f;

        // Looping once per set interval (nexttT), generate each valid dungeon found during the test and fabricate (if set) it
        // so that it can be seen within the test scene.
        foreach (var lvl in validLevels)
        {
            nextT = Time.realtimeSinceStartup + wait;

            print("Level: " + lvl);

            Generator.GenerateTestDungeon(lvl);

            if (fabricate) Generator.FabricateTest(lvl);

            yield return new WaitForSeconds(nextT - Time.realtimeSinceStartup);
        }
    }

    /// <summary>
    /// Returns true if the current level is the final level.
    /// </summary>
    /// <returns></returns>
    public bool IsFinalLevel()
    {
        return m_currentLevel == m_levelSeeds.Length - 1;
    }

    /// <summary>
    /// Setup before a level is loaded. Call this before LoadNext().
    /// </summary>
    public void NextLevelSetup()
    {
        m_currentLevel++;
        PersistentData.SaveInt(PersistentData.KEY_INT.LEVEL, m_currentLevel);
        Generator.InitNewLevel(m_currentLevel);
    }

    /// <summary>
    /// Loads the next level, semi-asynchronously
    /// </summary>
    public IEnumerator LoadNextAsync()
    {
        // Update the current dungeon in memory
        UpdateDungeon(m_levelSeeds[m_currentLevel]);

        // Update the preview texture.
        UpdatePreviewTexture();

        // Fabricate the dungeon
        Generator.Fabricate();

        // Wait for one frame to reduce the stutter after loading, and to ensure the next operation is performed
        // on the correct dungeon.
        yield return new WaitForFixedUpdate();

        // Generate the pathfinding grid based on the newly generated dungeon.
        m_grid.CreateGrid();

        // Set the locking bool to false to allow operations to continue elsewhere.
        IsLoadingAsync = false;
        yield return null;
    }

    /// <summary>
    /// Loads the next level. Make sure that NextLevelSetup() is called beforehand.
    /// </summary>
    public void LoadNext()
    {
        UpdateDungeon(m_levelSeeds[m_currentLevel]);
        UpdatePreviewTexture();

        Generator.Fabricate();
        m_grid.CreateGrid();
    }

    // Updates the current dungeon in memory.
    void UpdateDungeon(int seed)
    {
        Generator.GenerateNewDungeon(seed);
    }
    
    // Updates the preview texture for debugging purposes.
    void UpdatePreviewTexture()
    {
        var t = Generator.CurrentDungeon.Texture;
        m_previewPlane.GetComponent<MeshRenderer>().material.mainTexture = t;
    }
}
