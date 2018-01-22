using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DungeonGeneration
{
    /// <summary>
    /// Converts in-memory dungeon data into physical in-game objects. Initialise a new instance for each dungeon.
    /// </summary>
    public class Fabricator
    {
        // Reference to current dungeon layout in memory.
        readonly Dungeon m_dungeon;

        // Maximum number of enemies that can be placed on a platform.
        const int PLATFORM_MAX_ENEMIES = 6;

        //Various settings for the current dungeon.
        int m_dungeonScale;
        int m_positionOffset;
        int m_levelIndex;
        Vector2 m_startNode, m_endNode;
        Vector2? m_chestDirection;

        // Various references to GameObject's representing in-game units.
        GameObject m_playerCharacter;
        GameObject m_chest;
        GameObject m_boss;
        GameObject[] m_enemies;

        // Various references to materials used for dynamic texturing.
        Material[] m_platformCorners = new Material[5];
        Material[] m_platformEdges = new Material[5];
        Material[] m_paths = new Material[5];
        Material[] m_pathCorners = new Material[5];

        // Const bitmasks for orientation detection (for skinning blocks).
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

        // Helper property that calculates the current index, for dynamic texturing.
        int SkinIndex
        {
            get
            {
                return m_levelIndex % 5;
            }
        }

        /// <summary>
        /// Creates a new instance, loads all appropriate resources and sets up internal variables.
        /// </summary>
        public Fabricator(Dungeon d, int scale, int positionOffset, int lvl)
        {
            m_dungeon = d;
            m_startNode = d.Nodes[0].Center;
            m_endNode = d.Nodes[1].Center;
            m_dungeonScale = scale;
            m_positionOffset = positionOffset;
            m_levelIndex = lvl;

            m_playerCharacter = Resources.Load("Player/Knight") as GameObject;
            m_chest = Resources.Load("Chest/Chest") as GameObject;
            m_boss = Resources.Load("Enemies/Bosses/Boss1") as GameObject;

            m_enemies = new GameObject[4];
            for (int i = 0; i < 4; i++)
            {
                m_enemies[i] = Resources.Load("Enemies/Goblins/Goblin " + (i + 1)) as GameObject;
            }

            for (int i = 0; i < m_platformCorners.Length; i++)
            {
                m_platformCorners[i] = Resources.Load("Materials/CORNER" + (i + 1)) as Material;
                m_platformEdges[i] = Resources.Load("Materials/EDGE" + (i + 1)) as Material;
                m_paths[i] = Resources.Load("Materials/PATH" + (i + 1)) as Material;
                m_pathCorners[i] = Resources.Load("Materials/PATHCORNER" + (i + 1)) as Material;
            }
        }

        /// <summary>
        /// Updates the internal level Index.
        /// </summary>
        /// <param name="level"></param>
        public void UpdateLevel(int level)
        {
            m_levelIndex = level;
        }

        /// <summary>
        /// Fabricates but skips some unneccessary function calls, for testing.
        /// </summary>
        public void FabricateTest()
        {
            // Create a list of references to all the existing fabricated GameObjects, and remove them all.
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

            // Create a new empty GameObject to store tiles.
            var container = new GameObject("Tiles");

            // Generate the custom UV map.
            var cubeUV = UVMap();

            // For each row in the dungeon data, calculate the block to place, and place it.
            for (int i = 0; i < m_dungeon.Count; i++)
            {
                // Run through each row, tile by tile.
                for (int j = 0; j < m_dungeon[0].Length; j++)
                {
                    // Get the corresponding block, based on tile type.
                    var block = GetBlock(m_dungeon[j][i]);

                    // If a block is found (if not, it's empty space), spawn it, place, rotate and skin it.
                    if (block)
                    {
                        // Scale the stored coordinates to world coordinates.
                        var y = Scale(j);
                        var x = Scale(i);

                        // Instantiate the chosen tile.
                        var blockGameObject = UnityEngine.Object.Instantiate(block, new Vector3(x, 0, y), Quaternion.identity, container.transform) as GameObject;

                        // Calculate the block's skinning data in the form of a bitmask.
                        var blockState = GetBlockState(j, i);

                        // If the block is not a standard, central cube, skin it accordingly.
                        // Central cubes are skinned using the default UV map, as they are only ever seen from above.
                        // Therefore they are not modified in any way in the statement below.
                        if (blockState > 0)
                        {
                            // Apply the custom UV map.
                            blockGameObject.GetComponent<MeshFilter>().mesh.uv = cubeUV;

                            // Depending on the type of tile, rotate it and skin it.
                            if (m_dungeon[j][i] == m_dungeon.PathChar)
                            {
                                if (blockState % 3 == 0)
                                {
                                    blockGameObject.GetComponent<MeshRenderer>().material = m_pathCorners[SkinIndex];

                                    switch (blockState)
                                    {
                                        case NE_CORNER:
                                            blockGameObject.transform.Rotate(Vector3.up, -90);
                                            break;
                                        case SE_CORNER:
                                            // No rotation here, just leaving this case in for readibility
                                            break;
                                        case SW_CORNER:
                                            blockGameObject.transform.Rotate(Vector3.up, 90);
                                            break;
                                        case NW_CORNER:
                                            blockGameObject.transform.Rotate(Vector3.up, 180);
                                            break;
                                    }
                                }
                                else
                                {
                                    blockGameObject.GetComponent<MeshRenderer>().material = m_paths[SkinIndex];

                                    switch (blockState)
                                    {
                                        case NS_PATH:
                                            blockGameObject.transform.Rotate(Vector3.up, 180);
                                            break;
                                        case WE_PATH:
                                            blockGameObject.transform.Rotate(Vector3.up, -90);
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                if (blockState % 3 == 0)
                                {
                                    blockGameObject.GetComponent<MeshRenderer>().material = m_platformCorners[SkinIndex];

                                    switch (blockState)
                                    {
                                        case NE_CORNER:
                                            blockGameObject.transform.Rotate(Vector3.up, -90);
                                            break;
                                        case SE_CORNER:
                                            // No rotation here, just leaving this case in for readibility
                                            break;
                                        case SW_CORNER:
                                            blockGameObject.transform.Rotate(Vector3.up, 90);
                                            break;
                                        case NW_CORNER:
                                            blockGameObject.transform.Rotate(Vector3.up, 180);
                                            break;
                                    }
                                }
                                else
                                {
                                    blockGameObject.GetComponent<MeshRenderer>().material = m_platformEdges[SkinIndex];

                                    switch (blockState)
                                    {
                                        case N_EDGE:
                                            blockGameObject.transform.Rotate(Vector3.up, 180);
                                            break;
                                        case E_EDGE:
                                            blockGameObject.transform.Rotate(Vector3.up, -90);
                                            break;
                                        case S_EDGE:
                                            // No rotation here, just leaving this case in for readibility
                                            break;
                                        case W_EDGE:
                                            blockGameObject.transform.Rotate(Vector3.up, 90);
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Place enemies in the dungeon
            PlaceUnits(true);
        }

        /// <summary>
        /// Fabricates the dungeon by reading internal dungeon data.
        /// </summary>
        public void Fabricate()
        {
            // Remove the camera's reference to the player.
            CameraFollow.DereferencePlayerUnit();

            // Create a list of references to all the existing fabricated GameObjects, and remove them all.
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

            // Remove the platform manager's reference to the player.
            Platforms.UnregisterPlayer();

            // Create a new empty GameObject to store tiles.
            var container = new GameObject("Tiles");

            // Generate the custom UV map.
            var cubeUV = UVMap();

            // For each row in the dungeon data, calculate the block to place, and place it.
            for (int i = 0; i < m_dungeon.Count; i++)
            {
                // Run through each row, tile by tile.
                for (int j = 0; j < m_dungeon[0].Length; j++)
                {
                    // Get the corresponding block, based on tile type.
                    var block = GetBlock(m_dungeon[j][i]);

                    // If a block is found (if not, it's empty space), spawn it, place, rotate and skin it.
                    if (block)
                    {
                        // Scale the stored coordinates to world coordinates.
                        var y = Scale(j);
                        var x = Scale(i);

                        // Instantiate the chosen tile.
                        var tile = UnityEngine.Object.Instantiate(block, new Vector3(x, 0, y), Quaternion.identity, container.transform) as GameObject;

                        // Calculate the block's skinning data in the form of a bitmask.
                        var blockState = GetBlockState(j, i);

                        // If the block is not a standard, central cube, skin it accordingly.
                        // Central cubes are skinned using the default UV map, as they are only ever seen from above.
                        // Therefore they are not modified in any way in the statement below.
                        if (blockState > 0)
                        {
                            // Apply the custom UV map.
                            tile.GetComponent<MeshFilter>().mesh.uv = cubeUV;

                            // Depending on the type of tile, rotate it and skin it.
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

        /// <summary>
        /// Returns a custom UV map for skinning non-central tile.
        /// </summary>
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

        // Returns a bitmask representing 2^pow
        int GetMask(int pow)
        {
            const UInt16 TWOPZERO = 1;
            var v = TWOPZERO << pow;

            return v;
        }

        /// <summary>
        /// Returns a unique bitmask, representing the state of the block (by coordinates) stored in the internal dungeon object.
        /// </summary>
        /// <remarks>
        /// See const bitmask arguments at the top of this class for identification.
        /// Uses bitwise magic to efficiently create a unique mask, representing the type of the tile and its neighbours.
        /// The state of a tile is determined by it's neighbours, including blank spaces.
        /// The following observations allow for unique patterns to be generated for each block state.
        /// - Tiles touching the edges of the dungeon area are always edge tiles,
        /// - Path tiles that have two opposite adjacent blank/platform tiles are straight paths,
        /// - Path tiles that have two non-opposite adjacent blank/platform tiles are path corners,
        /// - Platform tiles with no adjacent non-platform tiles are always central tiles,
        /// - Platform tiles with one adjacent blank/path tile are always edges,
        /// - Platform tiles with two adjacent blank/path tiles are always corners.
        /// </remarks>
        int GetBlockState(int x, int y)
        {
            // Default value of 0 - Central tiles are returned with this.
            int blockState = 0;

            // If the tile is a path tile, calculate the mask and return.
            if (m_dungeon[x][y] == m_dungeon.PathChar)
            {
                // Capture edge cases
                if (y == m_dungeon.Count - 1) blockState = blockState | GetMask(0);         // Block is touching top edge
                if (x == m_dungeon[0].Length - 1) blockState = blockState | GetMask(1);     // Block is touching right edge
                if (y == 0) blockState = blockState | GetMask(2);                           // Block is touching bottom edge
                if (x == 0) blockState = blockState | GetMask(3);                           // Block is touching left edge

                // Then mask off the following states if valid:

                // If block is not touching the top edge, and that neighbour is blank...
                if ((blockState & GetMask(0)) == 0)
                {
                    if (m_dungeon[x][y + 1] == m_dungeon.EmptyChar) blockState = blockState | GetMask(0);
                }

                // If the block is not touching the right edge, and that neighbour is blank...
                if ((blockState & GetMask(1)) == 0)
                {
                    if (m_dungeon[x + 1][y] == m_dungeon.EmptyChar) blockState = blockState | GetMask(1);
                }

                // If block is not touching the bottom edge, and that neighbour is blank...
                if ((blockState & GetMask(2)) == 0)
                {
                    if (m_dungeon[x][y - 1] == m_dungeon.EmptyChar) blockState = blockState | GetMask(2);
                }

                // If the block is not touching the left edge, and that neighbour is blank...
                if ((blockState & GetMask(3)) == 0)
                {
                    if (m_dungeon[x - 1][y] == m_dungeon.EmptyChar) blockState = blockState | GetMask(3);
                }

                // Return the completed bitmask.
                return blockState;
            }

            // If the tile is not a pathchar, this section of code will generate the appropriate bitmask.

            // Capture edge cases
            if (y == m_dungeon.Count - 1) blockState = blockState | GetMask(0);             // Block is touching top edge
            if (x == m_dungeon[0].Length - 1) blockState = blockState | GetMask(1);         // Block is touching right edge
            if (y == 0) blockState = blockState | GetMask(2);                               // Block is touching bottom edge
            if (x == 0) blockState = blockState | GetMask(3);                               // Block is touching left edge

            // Then mask off the following states if valid:

            // If block is not touching the top edge, and that neighbour is blank or a path tile...
            if ((blockState & GetMask(0)) == 0)
            {
                if (m_dungeon[x][y + 1] != m_dungeon.PlatformChar && m_dungeon[x][y + 1] != m_dungeon.NodeChar) blockState = blockState | GetMask(0);
            }

            // If block is not touching the right edge, and that neighbour is blank or a path tile...
            if ((blockState & GetMask(1)) == 0)
            {
                if (m_dungeon[x + 1][y] != m_dungeon.PlatformChar && m_dungeon[x + 1][y] != m_dungeon.NodeChar) blockState = blockState | GetMask(1);
            }

            // If block is not touching the bottom edge, and that neighbour is blank or a path tile...
            if ((blockState & GetMask(2)) == 0)
            {
                if (m_dungeon[x][y - 1] != m_dungeon.PlatformChar && m_dungeon[x][y - 1] != m_dungeon.NodeChar) blockState = blockState | GetMask(2);
            }

            // If block is not touching the left edge, and that neighbour is blank or a path tile...
            if ((blockState & GetMask(3)) == 0)
            {
                if (m_dungeon[x - 1][y] != m_dungeon.PlatformChar && m_dungeon[x - 1][y] != m_dungeon.NodeChar) blockState = blockState | GetMask(3);
            }

            return blockState;
        }

        /// <summary>
        /// Place all the enemies for the level, based on current internal level index.
        /// </summary>
        public void PlaceUnits(bool test = false)
        {
            // Create an empty container for the enemy units.
            var container = new GameObject("Enemies");

            // Read current platform data, for later positioning validity checks.
            var platformData = Platforms.GetPlatformData();

            // Create a container for the enemy units that will be spawned. This is later passed to
            // the AIManager.
            var aiSet = new Dictionary<int, List<IAttackable>>();

            // Seed the random number generator. Though this seed is not random, it is highly likely to be unique across
            // each call of this function. This is required as the random number generated is seeded with set values 
            // during the dungeon generation process.
            UnityEngine.Random.InitState((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);

            // Create some placeholder variables for tracking platform number, and enemy ID (used for registering enemies
            // within the AIManager class).
            int pNumber = 0;
            int eNumber = 1;

            // Locate the start and endnodes for exclusion.
            var startNode = new Vector2(Scale((int)m_startNode.x), Scale((int)m_startNode.y));
            var endNode = new Vector2(Scale((int)m_endNode.x), Scale((int)m_endNode.y));

            // Determine the number of units to spawn per platform.
            int numToSpawn = Mathf.Min(2 + m_levelIndex, PLATFORM_MAX_ENEMIES);

            // Loop through each platform and place enemies accordingly.
            foreach (var platform in platformData)
            {
                // Create a new blank list of attacker interfaces.
                var attackers = new List<IAttackable>();

                // Create a new blank list of positions. A list is used to track duplicates.
                var positions = new List<Vector2>();

                // If the platform isnt a start or end node (a bounds check is performed to determine this), 
                // place enemies.
                if (!platform.IsInBounds(startNode) && !platform.IsInBounds(endNode))
                {
                    // For each enemy to spawn, randomly pick a position and see if it's valid. If it is invalid,
                    // Drop the index and try again.
                    for (int i = 0; i < numToSpawn; i++)
                    {
                        // Get a random index, to select a random enemy unit type.
                        int enemyIndex = UnityEngine.Random.Range(0, 4);

                        // Find a random location on the current platform.
                        var pos = platform.GetRandomLocationOnPlatform(2);

                        // Check to make sure that this position isn't being used it. If it is, drop the index and
                        // try again.
                        if (positions.Contains(pos)) { i--; continue; }

                        // If the position is valid, add it to the position list.
                        positions.Add(pos);

                        // Get a reference to the type of enemy unit to be spawned.
                        var enemyObj = m_enemies[enemyIndex];

                        // Instantiate the enemy unit
                        GameObject enemy = GameObject.Instantiate(enemyObj, new Vector3(pos.x, 1, pos.y), Quaternion.identity, container.transform);
                        var enemyClass = enemy.GetComponent<EnemyPathFindingObject>();

                        // Set the class to not running, so that it will do nothing until activated.
                        enemyClass.Running = false;

                        // Set the level of the enemy unit.
                        if (m_levelIndex == 0)
                        {
                            enemyClass.SetLevel(1);
                        }
                        else
                        {
                            enemyClass.SetLevel(m_levelIndex * 5);
                        }

                        // Find the IAttackable interface of the enemy unit and assign it an ID number.
                        var enemyClassInterface = enemyClass as IAttackable;
                        enemyClassInterface.ID = eNumber;

                        // Increment the enemy unit number.
                        eNumber++;

                        // Add the enemy unit interface to the attackers list.
                        attackers.Add(enemyClassInterface);
                    }
                }
                else if (platform.IsInBounds(endNode))
                {
                    // Special case: If this is a boss level (all multiples of 5 exluding 0), place a boss unit at the end node.
                    if (SkinIndex > 0 && (SkinIndex + 1) % 5 == 0)
                    {
                        // Create a new container for the boss.
                        var bossContainer = new GameObject("Boss");

                        // Instantiate the boss in at the center of the end node.
                        var boss = GameObject.Instantiate(m_boss, new Vector3(Scale((int)m_endNode.x), 1, Scale((int)m_endNode.y)), Quaternion.identity, bossContainer.transform);

                        // Find the closest path block.
                        var closestPathBlock = GameObject.FindGameObjectsWithTag("Path").OrderBy(i => (i.transform.position - boss.transform.position).sqrMagnitude).FirstOrDefault();

                        // Calculate the closest path blocks position.
                        var closestPathTileV2 = new Vector2(closestPathBlock.transform.position.x, closestPathBlock.transform.position.z);

                        // Calculate the boss position.
                        var bossTileV2 = new Vector2(boss.transform.position.x, boss.transform.position.z);

                        // Calculate the facing dir and align it to face UP DOWN LEFT or RIGHT. (+- X and Z).
                        Vector2 facingDir = (closestPathTileV2 - bossTileV2).Align2D();

                        // Turn the boss to face in the direction of the facing dir.
                        boss.transform.LookAt(boss.transform.position + new Vector3(facingDir.x, 0, facingDir.y));

                        // Get the boss class, set its level and prevent it from running.
                        var enemyClass = boss.GetComponent<EnemyPathFindingObject>();
                        enemyClass.SetLevel(m_levelIndex * 5);
                        enemyClass.Running = false;

                        // Extract the IAttackable interface, assign the ID and set the unit as a boss unit.
                        var enemyClassInterface = enemyClass as IAttackable;
                        enemyClassInterface.ID = eNumber;
                        enemyClassInterface.IsBossUnit = true;

                        // Add the reference to the enemy interface list.
                        attackers.Add(enemyClassInterface);
                    }
                    else
                    {
                        // If this is not a boss level, just place a chest instead.
                        var goal = GameObject.Instantiate(m_chest, new Vector3(Scale((int)m_endNode.x), 1, Scale((int)m_endNode.y)), Quaternion.identity);

                        var closestPathBlock = GameObject.FindGameObjectsWithTag("Path").OrderBy(i => (i.transform.position - goal.transform.position).sqrMagnitude).FirstOrDefault();

                        var closestPathTileV2 = new Vector2(closestPathBlock.transform.position.x, closestPathBlock.transform.position.z);
                        var chestTileV2 = new Vector2(goal.transform.position.x, goal.transform.position.z);

                        Vector2 facingDir = (closestPathTileV2 - chestTileV2).Align2D();

                        goal.transform.LookAt(goal.transform.position + new Vector3(facingDir.x, 0, facingDir.y));
                    }
                }
                else if (platform.IsInBounds(startNode))
                {
                    // If the node is the startnode, place the player.
                    GameObject.Instantiate(m_playerCharacter, new Vector3(Scale((int)m_startNode.x), 1, Scale((int)m_startNode.y)), Quaternion.identity);
                }

                // Add the complete platform enemy set to the AI set.
                aiSet.Add(pNumber, attackers);

                // increment the platform number.
                pNumber++;
            }

            // Load the created AISet into the AIManager.
            if (!test) AIManager.LoadAIData(new AISet(aiSet));
        }

        /// <summary>
        /// Scales a coordinate based on the dungeon scaling value.
        /// </summary>
        /// <param name="i"></param>
        int Scale(int i)
        {
            return (i * m_dungeonScale) - m_positionOffset + (m_dungeonScale / 2);
        }

        /// <summary>
        /// Returns the appropriate GameObject represented by the the dungeon tile (char) value passed in.
        /// </summary>
        GameObject GetBlock(char c)
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
