using System.Collections.Generic;
using UnityEngine;
using SilentKnight.AI;
using SilentKnight.Entities;
using SilentKnight.Utility;

namespace SilentKnight.DungeonGeneration
{
    /// <summary>
    /// Container for platform bounding data. Provides various helpers.
    /// </summary>
    public class PlatformBounds
    {
        // Two corners of the platform, from which all data can be extrapolated.
        public Vector2 BottomLeft { get; set; }
        public Vector2 TopRight { get; set; }

        // Should be true if the player is within the bounds of this platform.
        public bool PlayerIsWithinBounds { get; set; }

        public PlatformBounds(Vector2 bottomLeft, Vector2 topRight)
        {
            BottomLeft = bottomLeft;
            TopRight = topRight;

            PlayerIsWithinBounds = false;
        }

        /// <summary>
        /// Returns true if the position provided falls within the bounds of this platform.
        /// </summary>
        public bool IsInBounds(Vector2 pos)
        {
            if (pos.x < BottomLeft.x || pos.x > TopRight.x || pos.y < BottomLeft.y || pos.y > TopRight.y)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a random Vector2 location on this platform.
        /// </summary>
        public Vector2 GetRandomLocationOnPlatform(int padding)
        {
            int xRand = Random.Range(0 + padding, (int)TopRight.x - (int)BottomLeft.x - padding);
            int yRand = Random.Range(0 + padding, (int)TopRight.y - (int)BottomLeft.y - padding);

            return new Vector2(BottomLeft.x + xRand, BottomLeft.y + yRand);
        }
    }

    /// <summary>
    /// Container class for a list of platform data.
    /// </summary>
    public class Platforms : MonoBehaviour
    {
        // Players current platform by ID.
        public static int PlayerPlatform { get; private set; }

        // Container for platform data.
        public List<PlatformBounds> PlatformBounds { get; private set; }

        static Platforms instance;

        // Reference to player object.
        PlayerPathFindingObject m_player;

        void Awake()
        {
            instance = this;

            PlayerPlatform = -1;
        }

        void LateUpdate()
        {
            // Early exit if player is non-existent (dead, not yet instantiated etc.).
            if (!m_player) return;

            // Get the players current platform.
            var p = GetPlatformId(m_player.transform.position);

            // If -1, player is on a path.
            if (p != -1) PlayerPlatform = p;

            // If the player is entering a new platform, call the OnEnterPlatform() event on the player class.
            if (m_player.CurrentPlatformIndex != PlayerPlatform) m_player.OnEnterPlatform();

            // Set players current platform index.
            m_player.CurrentPlatformIndex = PlayerPlatform;

            // Update current player platform to be aware that the player is in its bounds.
            PlatformBounds[PlayerPlatform].PlayerIsWithinBounds = true;

            // Activate all units on the players platform.
            AIManager.ActivateUnits(PlayerPlatform);
        }

        /// <summary>
        /// Updates the platforms container with a reference to the current player unit.
        /// </summary>
        public static void RegisterPlayer(PlayerPathFindingObject player)
        {
            instance.m_player = player;
        }

        /// <summary>
        /// Unregisters the current player unit from the platform data.
        /// </summary>
        public static void UnregisterPlayer()
        {
            instance.m_player = null;
        }

        /// <summary>
        /// Returns a list of all platforms.
        /// </summary>
        public static List<PlatformBounds> GetPlatformData()
        {
            return instance.PlatformBounds;
        }

        /// <summary>
        /// Returns the nearest platform (as an ID) to a Vector3 world positon.
        /// </summary>
        public static int GetPlatformId(Vector3 pos)
        {
            int id = -1;

            for (int i = 0; i < instance.PlatformBounds.Count; i++)
            {
                if (instance.PlatformBounds[i].IsInBounds(pos.ToVector2()))
                {
                    id = i;
                }
                else
                {
                    instance.PlatformBounds[i].PlayerIsWithinBounds = false;
                }
            }

            return id;
        }

        /// <summary>
        /// Update the currently stored list of platforms.
        /// </summary>
        /// <param name="platforms"></param>
        public static void Update(List<PlatformBounds> platforms)
        {
            instance.PlatformBounds = new List<PlatformBounds>(platforms);
        }

        /// <summary>
        /// Handles drawing of platform boundaries in the editor scene window.
        /// </summary>
        void OnDrawGizmos()
        {
            if (PlatformBounds == null) return;
            foreach (var platform in PlatformBounds)
            {
                if (platform.PlayerIsWithinBounds) Gizmos.color = Color.green;
                else Gizmos.color = Color.magenta;

                Gizmos.DrawLine(new Vector3(platform.BottomLeft.x, 1.5f, platform.BottomLeft.y), new Vector3(platform.BottomLeft.x, 1.5f, platform.TopRight.y));
                Gizmos.DrawLine(new Vector3(platform.BottomLeft.x, 1.5f, platform.TopRight.y), new Vector3(platform.TopRight.x, 1.5f, platform.TopRight.y));
                Gizmos.DrawLine(new Vector3(platform.TopRight.x, 1.5f, platform.TopRight.y), new Vector3(platform.TopRight.x, 1.5f, platform.BottomLeft.y));
                Gizmos.DrawLine(new Vector3(platform.BottomLeft.x, 1.5f, platform.BottomLeft.y), new Vector3(platform.TopRight.x, 1.5f, platform.BottomLeft.y));
            }
        }
    }
}
