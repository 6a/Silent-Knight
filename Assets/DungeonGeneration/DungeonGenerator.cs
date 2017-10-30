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
    [SerializeField] int[] m_levelSeeds;

    [SerializeField] GameObject m_testPlatform;

    void Start ()
    {
        Generator.Init(m_maxDungeonWidth, m_maxDungeonHeight,
    new PlatformProperties(m_minWidth, m_maxWidth, m_minHeight, m_maxHeight),
    m_cycles, m_padding, m_minPlatforms, m_emptyChar, m_platformChar, m_nodeChar, m_pathChar);

        for (int i = 0; i < 0; i++)
        {
            UpdateTexturePreview();

            m_levelSeeds[0]++;
        }
        UpdateTexturePreview();
    }

    void Update ()
    {

	}

    private void UpdateTexturePreview()
    {
        Generator.GenerateNewDungeon(m_levelSeeds[0]);
        var t = Generator.CurrentDungeonTexture;

        m_testPlatform.GetComponent<MeshRenderer>().material.mainTexture = t;
    }
}
