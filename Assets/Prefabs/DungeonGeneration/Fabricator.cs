using Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace DungeonGeneration
{
    public class Fabricator
    {
        readonly Dungeon m_dungeon;

        const int PLATFORM_MAX_ENEMIES = 6;

        public int DungeonScale;
        public int PositionOffset;

        Vector2 m_startNode, m_endNode;
        Vector2? m_chestDirection;
        GameObject m_playerCharacter;
        GameObject m_chest;
        GameObject m_boss;
        GameObject[] m_enemies;
        int m_levelIndex;

        Material[] m_platformCorners = new Material[5];
        Material[] m_platformEdges = new Material[5];
        Material[] m_paths = new Material[5];
        Material[] m_pathCorners = new Material[5];

        public int SkinIndex
        {
            get
            {
                return m_levelIndex % 5;
            }
            private set { }
        }

        public Fabricator(Dungeon d, int scale, int positionOffset, int lvl)
        {
            m_dungeon = d;
            m_startNode = d.Nodes[0].Center;
            m_endNode = d.Nodes[1].Center;
            DungeonScale = scale;
            PositionOffset = positionOffset;
            m_levelIndex = lvl;

            m_playerCharacter = Resources.Load("Player/Knight") as GameObject;
            m_chest = Resources.Load("Chest/Chest") as GameObject;
            m_boss = Resources.Load("Bosses/Boss1") as GameObject;

            m_enemies = new GameObject[4];
            for (int i = 0; i < 4; i++)
            {
                m_enemies[i] = Resources.Load("Goblins/Goblin " + (i + 1)) as GameObject;
            }

            for (int i = 0; i < m_platformCorners.Length; i++)
            {
                m_platformCorners[i] = Resources.Load("Materials/CORNER" + (i + 1)) as Material;
                m_platformEdges[i] = Resources.Load("Materials/EDGE" + (i + 1)) as Material;
                m_paths[i] = Resources.Load("Materials/PATH" + (i + 1)) as Material;
                m_pathCorners[i] = Resources.Load("Materials/PATHCORNER" + (i + 1)) as Material;
            }
        }

        public void UpdateLevel(int level)
        {
            m_levelIndex = level;
        }

        public void FabricateTest()
        {
            var oldObjects = new List<GameObject>
            {
                GameObject.Find("Tiles"),
                GameObject.FindGameObjectWithTag("Player"),
                GameObject.FindGameObjectWithTag("Chest"),
                GameObject.Find("Enemies"),
                GameObject.Find("Sparky"),
                GameObject.Find("Boss")
            };

            foreach (var ob in oldObjects)
            {
                if (ob != null) GameObject.Destroy(ob);
            }

            var container = new GameObject("Tiles");

            var cubeUV = UVMap();

            for (int i = 0; i < m_dungeon.Count; i++)
            {
                for (int j = 0; j < m_dungeon[0].Length; j++)
                {
                    var block = GetBlock(m_dungeon[j][i]);

                    if (block)
                    {
                        var y = Scale(j);
                        var x = Scale(i);

                        var tile = UnityEngine.Object.Instantiate(block, new Vector3(x, 0, y), Quaternion.identity, container.transform) as GameObject;

                        var blockState = GetBlockState(j, i);

                        if (blockState > 0)
                        {
                            tile.GetComponent<MeshFilter>().mesh.uv = cubeUV;

                            if (m_dungeon[j][i] == m_dungeon.PathChar)
                            {
                                if (blockState % 3 == 0)
                                {
                                    tile.GetComponent<MeshRenderer>().material = m_pathCorners[SkinIndex];

                                    switch (blockState)
                                    {
                                        case NE_CORNER:
                                            tile.transform.Rotate(Vector3.up, -90);
                                            break;
                                        case SE_CORNER:
                                            // No rotation here, just leaving this case in for readibility
                                            break;
                                        case SW_CORNER:
                                            tile.transform.Rotate(Vector3.up, 90);
                                            break;
                                        case NW_CORNER:
                                            tile.transform.Rotate(Vector3.up, 180);
                                            break;
                                    }
                                }
                                else
                                {
                                    tile.GetComponent<MeshRenderer>().material = m_paths[SkinIndex];

                                    switch (blockState)
                                    {
                                        case NS_PATH:
                                            tile.transform.Rotate(Vector3.up, 180);
                                            break;
                                        case WE_PATH:
                                            tile.transform.Rotate(Vector3.up, -90);
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                if (blockState % 3 == 0)
                                {
                                    tile.GetComponent<MeshRenderer>().material = m_platformCorners[SkinIndex];

                                    switch (blockState)
                                    {
                                        case NE_CORNER:
                                            tile.transform.Rotate(Vector3.up, -90);
                                            break;
                                        case SE_CORNER:
                                            // No rotation here, just leaving this case in for readibility
                                            break;
                                        case SW_CORNER:
                                            tile.transform.Rotate(Vector3.up, 90);
                                            break;
                                        case NW_CORNER:
                                            tile.transform.Rotate(Vector3.up, 180);
                                            break;
                                    }
                                }
                                else
                                {
                                    tile.GetComponent<MeshRenderer>().material = m_platformEdges[SkinIndex];

                                    switch (blockState)
                                    {
                                        case N_EDGE:
                                            tile.transform.Rotate(Vector3.up, 180);
                                            break;
                                        case E_EDGE:
                                            tile.transform.Rotate(Vector3.up, -90);
                                            break;
                                        case S_EDGE:
                                            // No rotation here, just leaving this case in for readibility
                                            break;
                                        case W_EDGE:
                                            tile.transform.Rotate(Vector3.up, 90);
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            PlaceEnemies(true);
            PlaceGoal();
            PlacePlayerAtStartNode();
            Finalise();
        }

        public void Fabricate()
        {
            CameraFollow.DereferenceKnight();

            var oldObjects = new List<GameObject>
            {
                GameObject.Find("Tiles"),
                GameObject.FindGameObjectWithTag("Player"),
                GameObject.FindGameObjectWithTag("Chest"),
                GameObject.Find("Enemies"),
                GameObject.Find("Sparky")
            };

            foreach (var ob in oldObjects)
            {
                if (ob != null) GameObject.Destroy(ob);
            }

            Platforms.UnregisterPlayer();

            var container = new GameObject("Tiles");

            var cubeUV = UVMap();

            for (int i = 0; i < m_dungeon.Count; i++)
            {
                for (int j = 0; j < m_dungeon[0].Length; j++)
                {
                    var block = GetBlock(m_dungeon[j][i]);

                    if (block)
                    {
                        var y = Scale(j);
                        var x = Scale(i);

                        var tile = UnityEngine.Object.Instantiate(block, new Vector3(x, 0, y), Quaternion.identity, container.transform) as GameObject;

                        var blockState = GetBlockState(j, i);

                        if (blockState > 0)
                        {
                            tile.GetComponent<MeshFilter>().mesh.uv = cubeUV;

                            if (m_dungeon[j][i] == m_dungeon.PathChar)
                            {
                                if (blockState % 3 == 0)
                                {
                                    tile.GetComponent<MeshRenderer>().material = m_pathCorners[SkinIndex];

                                    switch (blockState)
                                    {
                                        case NE_CORNER:
                                            tile.transform.Rotate(Vector3.up, -90);
                                            break;
                                        case SE_CORNER:
                                            // No rotation here, just leaving this case in for readibility
                                            break;
                                        case SW_CORNER:
                                            tile.transform.Rotate(Vector3.up, 90);
                                            break;
                                        case NW_CORNER:
                                            tile.transform.Rotate(Vector3.up, 180);
                                            break;
                                    }
                                }
                                else
                                {
                                    tile.GetComponent<MeshRenderer>().material = m_paths[SkinIndex];

                                    switch (blockState)
                                    {
                                        case NS_PATH:
                                            tile.transform.Rotate(Vector3.up, 180);
                                            break;
                                        case WE_PATH:
                                            tile.transform.Rotate(Vector3.up, -90);
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                if (blockState % 3 == 0)
                                {
                                    tile.GetComponent<MeshRenderer>().material = m_platformCorners[SkinIndex];

                                    switch (blockState)
                                    {
                                        case NE_CORNER:
                                            tile.transform.Rotate(Vector3.up, -90);
                                            break;
                                        case SE_CORNER:
                                            // No rotation here, just leaving this case in for readibility
                                            break;
                                        case SW_CORNER:
                                            tile.transform.Rotate(Vector3.up, 90);
                                            break;
                                        case NW_CORNER:
                                            tile.transform.Rotate(Vector3.up, 180);
                                            break;
                                    }
                                }
                                else
                                {
                                    tile.GetComponent<MeshRenderer>().material = m_platformEdges[SkinIndex];

                                    switch (blockState)
                                    {
                                        case N_EDGE:
                                            tile.transform.Rotate(Vector3.up, 180);
                                            break;
                                        case E_EDGE:
                                            tile.transform.Rotate(Vector3.up, -90);
                                            break;
                                        case S_EDGE:
                                            // No rotation here, just leaving this case in for readibility
                                            break;
                                        case W_EDGE:
                                            tile.transform.Rotate(Vector3.up, 90);
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        Vector2[] UVMap()
        {
            var UVs = new Vector2[24];

            // Front
            UVs[7] = new Vector2(0.0f, 0.0f);
            UVs[6] = new Vector2(1, 0.0f);
            UVs[11] = new Vector2(0, 0.5f);
            UVs[10] = new Vector2(1, 0.5f);
            // Top
            UVs[5] = new Vector2(0, 0.5f);
            UVs[4] = new Vector2(1, 0.5f);
            UVs[9] = new Vector2(0, 1);
            UVs[8] = new Vector2(1, 1);
            // Back
            UVs[0] = new Vector2(0.0f, 0.0f);
            UVs[1] = new Vector2(1, 0.0f);
            UVs[2] = new Vector2(0, 0.5f);
            UVs[3] = new Vector2(1, 0.5f);
            // Bottom
            UVs[13] = new Vector2(0.0f, 0.0f);
            UVs[12] = new Vector2(1, 0.0f);
            UVs[14] = new Vector2(0, 0.5f);
            UVs[15] = new Vector2(1, 0.5f);
            // Left
            UVs[16] = new Vector2(0.0f, 0.0f);
            UVs[17] = new Vector2(0, 0.5f);
            UVs[18] = new Vector2(1, 0.5f);
            UVs[19] = new Vector2(1, 0);
            // Right        
            UVs[20] = new Vector2(0.0f, 0.0f);
            UVs[21] = new Vector2(0, 0.5f);
            UVs[22] = new Vector2(1, 0.5f);
            UVs[23] = new Vector2(1, 0);

            return UVs;
        }

        const UInt16 NE_CORNER = 0x3;
        const UInt16 SE_CORNER = 0x9;
        const UInt16 SW_CORNER = 0xC;
        const UInt16 NW_CORNER = 0x6;

        const UInt16 N_EDGE = 0x2;
        const UInt16 E_EDGE = 0x1;
        const UInt16 S_EDGE = 0x8;
        const UInt16 W_EDGE = 0x4;

        const UInt16 NS_PATH = 0x5;
        const UInt16 WE_PATH = 0xA;

        int GetMask(int pow)
        {
            const UInt16 TWOPZERO = 1;
            var v = TWOPZERO << pow;

            return v;
        }

        int GetBlockState(int x, int y)
        {
            int blockState = 0;

            if (m_dungeon[x][y] == m_dungeon.PathChar)
            {
                if (y == m_dungeon.Count - 1) blockState = blockState | GetMask(0);
                if (x == m_dungeon[0].Length - 1) blockState = blockState | GetMask(1);
                if (y == 0) blockState = blockState | GetMask(2);
                if (x == 0) blockState = blockState | GetMask(3);

                if ((blockState & GetMask(0)) == 0)
                {
                    if (m_dungeon[x][y + 1] == m_dungeon.EmptyChar) blockState = blockState | GetMask(0);
                }

                if ((blockState & GetMask(1)) == 0)
                {
                    if (m_dungeon[x + 1][y] == m_dungeon.EmptyChar) blockState = blockState | GetMask(1);
                }

                if ((blockState & GetMask(2)) == 0)
                {
                    if (m_dungeon[x][y - 1] == m_dungeon.EmptyChar) blockState = blockState | GetMask(2);
                }

                if ((blockState & GetMask(3)) == 0)
                {
                    if (m_dungeon[x - 1][y] == m_dungeon.EmptyChar) blockState = blockState | GetMask(3);
                }

                return blockState;
            }

            if (y == m_dungeon.Count - 1) blockState = blockState | GetMask(0);
            if (x == m_dungeon[0].Length - 1) blockState = blockState | GetMask(1);
            if (y == 0) blockState = blockState | GetMask(2);
            if (x == 0) blockState = blockState | GetMask(3);

            if ((blockState & GetMask(0)) == 0)
            {
                if (m_dungeon[x][y + 1] != m_dungeon.PlatformChar && m_dungeon[x][y + 1] != m_dungeon.NodeChar) blockState = blockState | GetMask(0);
            }

            if ((blockState & GetMask(1)) == 0)
            {
                if (m_dungeon[x + 1][y] != m_dungeon.PlatformChar && m_dungeon[x + 1][y] != m_dungeon.NodeChar) blockState = blockState | GetMask(1);
            }

            if ((blockState & GetMask(2)) == 0)
            {
                if (m_dungeon[x][y - 1] != m_dungeon.PlatformChar && m_dungeon[x][y - 1] != m_dungeon.NodeChar) blockState = blockState | GetMask(2);
            }

            if ((blockState & GetMask(3)) == 0)
            {
                if (m_dungeon[x - 1][y] != m_dungeon.PlatformChar && m_dungeon[x - 1][y] != m_dungeon.NodeChar) blockState = blockState | GetMask(3);
            }

            return blockState;
        }

        public void PlaceEnemies(bool test = false)
        {
            var container = new GameObject("Enemies");

            var platformData = Platforms.GetPlatformData();

            var aiSet = new Dictionary<int, List<IAttackable>>();

            UnityEngine.Random.InitState((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);

            int pNumber = 0;
            int eNumber = 1;
            foreach (var platform in platformData)
            {
                var attackers = new List<IAttackable>();

                var startNode = new Vector2(Scale((int)m_startNode.x), Scale((int)m_startNode.y));
                var endNode = new Vector2(Scale((int)m_endNode.x), Scale((int)m_endNode.y));

                int numToSpawn = Mathf.Min(2 + m_levelIndex, PLATFORM_MAX_ENEMIES);
                var positions = new List<Vector2>();

                if (!platform.IsInBounds(startNode) && !platform.IsInBounds(endNode))
                {
                    for (int i = 0; i < numToSpawn; i++)
                    {
                        int r = UnityEngine.Random.Range(0, 4);

                        var pos = platform.GetRandomLocationOnPlatform(2);

                        if (positions.Contains(pos)) { i--; continue; }

                        positions.Add(pos);

                        var enemyObj = m_enemies[r];

                        GameObject enemy = GameObject.Instantiate(enemyObj, new Vector3(pos.x, 1, pos.y), Quaternion.identity);
                        enemy.transform.parent = container.transform;
                        var enemyClass = enemy.GetComponent<JEnemyUnit>();

                        enemyClass.Running = false;

                        if (m_levelIndex == 0)
                        {
                            enemyClass.SetLevel(1);
                        }
                        else
                        {
                            enemyClass.SetLevel(m_levelIndex * 5);
                        }

                        var enemyClassInterface = enemyClass as IAttackable;
                        enemyClassInterface.ID = eNumber;
                        eNumber++;
                        attackers.Add(enemyClassInterface);
                    }
                }
                else if (platform.IsInBounds(endNode))
                {
                    if (SkinIndex > 0 && (SkinIndex + 1) % 5 == 0)
                    {
                        var goal = GameObject.Instantiate(m_boss, new Vector3(Scale((int)m_endNode.x), 1, Scale((int)m_endNode.y)), Quaternion.identity);

                        var closestPathBlock = GameObject.FindGameObjectsWithTag("Path").OrderBy(i => (i.transform.position - goal.transform.position).sqrMagnitude).FirstOrDefault();

                        var closestPathTileV2 = new Vector2(closestPathBlock.transform.position.x, closestPathBlock.transform.position.z);
                        var chestTileV2 = new Vector2(goal.transform.position.x, goal.transform.position.z);

                        Vector2 facingDir = (closestPathTileV2 - chestTileV2).Snap();

                        goal.transform.LookAt(goal.transform.position + new Vector3(facingDir.x, 0, facingDir.y));

                        var enemyClass = goal.GetComponent<JEnemyUnit>();
                        enemyClass.SetLevel(m_levelIndex * 5);
                        enemyClass.Running = false;

                        var enemyClassInterface = enemyClass as IAttackable;
                        enemyClassInterface.ID = eNumber;
                        enemyClassInterface.IsBossUnit = true;

                        attackers.Add(enemyClassInterface);

                        var bossContainer = new GameObject("Boss");

                        enemyClass.gameObject.transform.parent = bossContainer.transform;
                    }
                }

                aiSet.Add(pNumber, attackers);

                pNumber++;
            }

            if (!test) AI.LoadAIData(new AISet(aiSet));
        }

        public void PlaceGoal()
        {
            if (SkinIndex > 0 && (SkinIndex + 1) % 5 == 0) return;

            var goal = GameObject.Instantiate(m_chest, new Vector3(Scale((int)m_endNode.x), 1, Scale((int)m_endNode.y)), Quaternion.identity);

            var closestPathBlock = GameObject.FindGameObjectsWithTag("Path").OrderBy(i => (i.transform.position - goal.transform.position).sqrMagnitude).FirstOrDefault();

            var closestPathTileV2 = new Vector2(closestPathBlock.transform.position.x, closestPathBlock.transform.position.z);
            var chestTileV2 = new Vector2(goal.transform.position.x, goal.transform.position.z);

            Vector2 facingDir = (closestPathTileV2 - chestTileV2).Snap();

            goal.transform.LookAt(goal.transform.position + new Vector3(facingDir.x, 0, facingDir.y));
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
                name += "Platform/Platform " + (SkinIndex + 1);
            }
            else if (c == m_dungeon.NodeChar)
            {
                name += "Node/Node " + (SkinIndex + 1);
            }
            else if (c == m_dungeon.PathChar)
            {
                name += "Path/Path " + (SkinIndex + 1);
            }
            else
            {
                return null;
            }
            return Resources.Load(name) as GameObject;
        }
    }
}
