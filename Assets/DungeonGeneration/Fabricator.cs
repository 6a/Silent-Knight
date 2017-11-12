using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DungeonGeneration
{
    public class Fabricator
    {
        readonly Dungeon m_dungeon;

        public int DungeonScale;
        public int PositionOffset;

        Vector2 m_startNode, m_endNode;
        Vector2? m_chestDirection;
        GameObject m_playerCharacter;
        GameObject m_chest;
        int m_levelIndex = 1;

        public Fabricator(Dungeon d, int scale, int positionOffset)
        {
            m_dungeon = d;
            m_startNode = d.Nodes[0].Center;
            m_endNode = d.Nodes[1].Center;
            DungeonScale = scale;
            PositionOffset = positionOffset;

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
                if (ob != null) GameObject.Destroy(ob);
            }

            Platforms.UnregisterPlayer();

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

                        var tile = GameObject.Instantiate(block, new Vector3(x, 0, y), Quaternion.identity, container.transform);
                    }
                }
            }
        }

        public void PlaceEnemies()
        {
            var platformData = Platforms.GetPlatformData();

            var aiSet = new Dictionary<int, List<IAttackable>>();

            int pNumber = 0;
            foreach (var platform in platformData)
            {
                var attackers = new List<IAttackable>();

                if (!platform.IsInBounds(m_startNode) && !platform.IsInBounds(m_endNode))
                {
                    var enemy1 = Resources.Load("Goblins/Goblin 1") as GameObject;

                    var pos = platform.GetRandomLocationOnPlatform(4);

                    GameObject enemy = GameObject.Instantiate(enemy1, new Vector3(pos.x, 1, pos.y), Quaternion.identity);

                    var enemyClass = enemy.GetComponent<JGoblinControl>();

                    enemyClass.Running = false;

                    attackers.Add(enemyClass as IAttackable);
                }

                aiSet.Add(pNumber, attackers);

                pNumber++;
            }

            AI.LoadAIData(new AISet(aiSet));
        }

        public void PlaceChestAtEndNode()
        {
            GameObject chest = GameObject.Instantiate(m_chest, new Vector3(Scale((int)m_endNode.x), 1, Scale((int)m_endNode.y)), Quaternion.identity);

            var closestPathBlock = GameObject.FindGameObjectsWithTag("Path").OrderBy(i => (i.transform.position - chest.transform.position).sqrMagnitude).FirstOrDefault();

            var closestPathTileV2 = new Vector2(closestPathBlock.transform.position.x, closestPathBlock.transform.position.z);
            var chestTileV2 = new Vector2(chest.transform.position.x, chest.transform.position.z);

            Vector2 facingDir = (closestPathTileV2 - chestTileV2).Snap();

            chest.transform.LookAt(chest.transform.position + new Vector3(facingDir.x, 0, facingDir.y));
        }

        public void PlacePlayerAtStartNode()
        {
            GameObject.Instantiate(m_playerCharacter, new Vector3(Scale((int)m_startNode.x), 1, Scale((int)m_startNode.y)), Quaternion.identity);
        }

        public void Finalise()
        {
            m_levelIndex++;
        }

        private int Scale(int i)
        {
            return (i * DungeonScale) - PositionOffset + (DungeonScale / 2);
        }

        private GameObject GetBlock(char c)
        {
            string name = "Blocks/";

            if (c == m_dungeon.PlatformChar)
            {
                name += "Platform/Platform " + m_levelIndex;
            }
            else if (c == m_dungeon.NodeChar)
            {
                name += "Node/Node " + m_levelIndex;
            }
            else if (c == m_dungeon.PathChar)
            {
                name += "Path/Path " + m_levelIndex;
            }
            else
            {
                return null;
            }

            return Resources.Load(name) as GameObject;
        }
    }
}
