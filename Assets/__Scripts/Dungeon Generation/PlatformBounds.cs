using UnityEngine;

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
}
