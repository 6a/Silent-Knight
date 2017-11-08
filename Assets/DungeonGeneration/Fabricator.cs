using System.Collections.Generic;
using System.Linq;
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

        Vector2 m_startNode, m_endNode;
        Vector2? m_chestDirection;
        GameObject m_playerCharacter;
        GameObject m_chest;

        public Fabricator(Dungeon d)
        {
            m_dungeon = d;
            m_startNode = d.Nodes[0].Center;
            m_endNode = d.Nodes[1].Center;

            m_playerCharacter = Resources.Load("Tetsuo/Knight") as GameObject;
            m_chest = Resources.Load("Chest/Chest") as GameObject;
        }

        public void Fabricate()
        {
            var oldObjects = new List<GameObject>();

            oldObjects.Add(GameObject.Find("Tiles"));
            oldObjects.Add(GameObject.FindGameObjectWithTag("Player"));
            oldObjects.Add(GameObject.FindGameObjectWithTag("Chest"));

            foreach (var ob in oldObjects)
            {
                if (ob != null) Object.Destroy(ob);
            }

            var container = new GameObject("Tiles");

            for (int i = 0; i < m_dungeon.Count; i++)
            {
                for (int j = 0; j < m_dungeon[0].Length; j++)
                {
                    var block = GetBlock(m_dungeon[j][i]);

                    if (block)
                    {
                        var y = Scale(j);
                        var x = Scale(i);

                        var tile = Object.Instantiate(block, new Vector3(x, 0, y), Quaternion.identity, container.transform);
                    }
                }
            }
        }

        public void PlaceChestAtEndNode()
        {
            GameObject chest = Object.Instantiate(m_chest, new Vector3(Scale((int)m_endNode.x), 1, Scale((int)m_endNode.y)), Quaternion.identity);

            var closestPathBlock = GameObject.FindGameObjectsWithTag("Path").OrderBy(i => (i.transform.position - chest.transform.position).sqrMagnitude).FirstOrDefault();


            var closestPathTileV2 = new Vector2(closestPathBlock.transform.position.x, closestPathBlock.transform.position.z);
            var chestTileV2 = new Vector2(chest.transform.position.x, chest.transform.position.z);

            Vector2 facingDir = (closestPathTileV2 - chestTileV2).Snap();

            chest.transform.LookAt(chest.transform.position + new Vector3(facingDir.x, 0, facingDir.y));
        }

        public void PlacePlayerAtStartNode()
        {
            Object.Instantiate(m_playerCharacter, new Vector3(Scale((int)m_startNode.x), 1, Scale((int)m_startNode.y)), Quaternion.identity);
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
