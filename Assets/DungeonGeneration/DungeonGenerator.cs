using DungeonGeneration;
using UnityEngine;
using PathFinding;

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
    [SerializeField] int [] m_levelSeeds;

    [SerializeField] int m_iterations;

    [SerializeField] GameObject m_previewPlane;

    int m_currentLevel;

    ASGrid m_grid;

    void Awake ()
    {
        m_grid = FindObjectOfType<ASGrid>();

        m_currentLevel = 0;

        Generator.Init(m_maxDungeonWidth, m_maxDungeonHeight,
        new PlatformProperties(m_minWidth, m_maxWidth, m_minHeight, m_maxHeight),
        m_cycles, m_padding, m_minPlatforms, m_emptyChar, m_platformChar, m_nodeChar, m_pathChar);
    }

    void Update ()
    {

    }

    public void LoadNext()
    {
        for (int i = 0; i < m_iterations; i++)
        {
            if (i > 0) m_levelSeeds[0]++;

            UpdateDungeon();
            UpdatePreviewTexture();

            if (i > 0 && Generator.CurrentDungeon.Nodes.Count == 2 && Generator.CurrentDungeon.Platforms.Count == 10)
            {
                Debug.Log("Suitable dungeon: " + m_levelSeeds[m_currentLevel]);
            }
        }

        Generator.Fabricate();
        m_grid.CreateGrid();
    }

    void UpdateDungeon()
    {
        Generator.GenerateNewDungeon(m_levelSeeds[m_currentLevel]);
    }
    
    void UpdatePreviewTexture()
    {
        var t = Generator.CurrentDungeon.Texture;
        m_previewPlane.GetComponent<MeshRenderer>().material.mainTexture = t;
    }
}
