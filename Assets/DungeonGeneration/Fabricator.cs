using UnityEngine;

namespace DungeonGeneration
{
    public class Fabricator
    {
        readonly Dungeon m_dungeon;

        const int m_numberOfPlatformBlocks = 4;
        const int m_numberOfNodeBlocks = 4;
        const int m_numberOfPathBlocks = 4;
        const int m_scale = 2;
        const int m_offset = 32;

        Vector2? m_startNode, m_endNode;
        GameObject m_playerCharacter;

        public Fabricator(Dungeon d)
        {
            m_dungeon = d;
            m_startNode = d.Nodes[0].Center;
            m_endNode = d.Nodes[1].Center;
            m_playerCharacter = GameObject.FindGameObjectWithTag("Player");
        }

        public void Fabricate()
        {
            for (int i = 0; i < m_dungeon.Count; i++)
            {
                for (int j = 0; j < m_dungeon[0].Length; j++)
                {
                    var block = GetBlock(m_dungeon[j][i]);

                    if (block)
                    {
                        var y = Scale(j);
                        var x = Scale(i);

                        Object.Instantiate(block, new Vector3(x, 0, y), Quaternion.identity);
                    }
                }
            }
        }

        public void PlacePlayerAtStartNode()
        {
            Debug.Log(m_startNode.Value);
            m_playerCharacter.transform.position = new Vector3(Scale((int)m_startNode.Value.x), 2, Scale((int)m_startNode.Value.y));
        }

        private int Scale(int i)
        {
            return (i * m_scale) - m_offset + (m_scale / 2);
        }

        private GameObject GetBlock(char c)
        {
            string name = "Blocks/";

            if (c == m_dungeon.PlatformChar)
            {
                name += "Platform/Platform " + Random.Range(1, m_numberOfPlatformBlocks);
            }
            else if (c == m_dungeon.NodeChar)
            {
                name += "Node/Node " + Random.Range(1, m_numberOfNodeBlocks);
            }
            else if (c == m_dungeon.PathChar)
            {
                name += "Path/Path " + Random.Range(1, m_numberOfPathBlocks);
            }
            else
            {
                return null;
            }
            return Resources.Load(name) as GameObject;
        }
    }
}
