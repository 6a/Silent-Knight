using System;

namespace SilentKnight.Utility
{
    /// <summary>
    /// Container class for various extensions
    /// </summary>
    public static class Extension
    {
        /// <summary>
        /// Aligns a 2D directional vector to the closest, normalised axis aligned direction [Left, right, up, or down].
        /// </summary>
        public static UnityEngine.Vector2 Align2D(this UnityEngine.Vector2 v)
        {
            v = v.normalized;

            var Y = v.y;
            var X = v.x;

            if (UnityEngine.Mathf.Abs(Y) >= UnityEngine.Mathf.Abs(X))
            {
                if (Y > 0)
                {
                    return UnityEngine.Vector2.up;
                }
                else
                {
                    return UnityEngine.Vector2.down;
                }
            }
            else
            {
                if (X > 0)
                {
                    return UnityEngine.Vector2.right;
                }
                else
                {
                    return UnityEngine.Vector2.left;
                }
            }
        }

        /// <summary>
        /// Converts a top down Vector3 to a planar Vector2. [X becomes X, Z becomes Y].
        /// </summary>
        public static UnityEngine.Vector2 ToVector2(this UnityEngine.Vector3 v)
        {
            return new UnityEngine.Vector2(v.x, v.z);
        }

        /// <summary>
        /// Formats a floating point number, truncating it to K, M, B etc.
        /// </summary>
        public static string Format(this float f, string suffix)
        {
            const float BILLION = 1000000000;
            const float MILLION = 1000000;
            const float THOUSAND = 1000;

            if (f >= BILLION) return Math.Round(f / BILLION, 3).ToString() + "B";
            else if (f >= MILLION) return Math.Round(f / MILLION, 3).ToString() + "M";
            else if (f >= THOUSAND) return Math.Round(f / THOUSAND, 2).ToString() + "K";
            else return Math.Round(f, 1).ToString() + suffix;
        }

        /// <summary>
        /// Recursively Sets the layer for gameobject and all of its children.
        /// </summary>
        public static void SetLayerRecursively(this UnityEngine.GameObject obj, int layer)
        {
            obj.layer = layer;

            foreach (UnityEngine.Transform child in obj.transform)
            {
                child.gameObject.SetLayerRecursively(layer);
            }
        }
    }
}