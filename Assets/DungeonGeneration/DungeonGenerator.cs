using DungeonGeneration;
using UnityEngine;

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
    [SerializeField] bool m_generate;

    [SerializeField] GameObject m_testPlatform;

    void Start ()
    {
        m_generate = false;

        Generator.Init(m_maxDungeonWidth, m_maxDungeonHeight,
    new PlatformProperties(m_minWidth, m_maxWidth, m_minHeight, m_maxHeight),
    m_cycles, m_padding, m_minPlatforms, m_emptyChar, m_platformChar, m_nodeChar, m_pathChar);

        for (int i = 0; i < m_iterations; i++)
        {
            UpdateDungeon();
            UpdatePreviewTexture();

            if (Generator.CurrentDungeon.Nodes.Count == 2 && Generator.CurrentDungeon.Platforms.Count == 10)
            {
                Debug.Log("Suitable dungeon: " + m_levelSeeds[0]);
            }

            m_levelSeeds[0]++;
        }
    }

    void Update ()
    {
        if (m_generate)
        {
            m_generate = false;
            UpdateDungeon();
            UpdatePreviewTexture();
        }
	}

    private void UpdateDungeon()
    {
        Generator.GenerateNewDungeon(m_levelSeeds[0]);
    }
    
    private void UpdatePreviewTexture()
    {
        var t = Generator.CurrentDungeon.Texture;
        m_testPlatform.GetComponent<MeshRenderer>().material.mainTexture = t;
    }
}
