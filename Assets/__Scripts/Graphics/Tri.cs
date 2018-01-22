using UnityEngine;

namespace SilentKnight.Graphics
{
    /// <summary>
    /// Container representing 3 points in world space.
    /// </summary>
    class Tri
    {
        /// <summary>
        /// Returns a point in world space representing the center of this triangle.
        /// </summary>
        public Vector3 Center
        {
            get
            {
                var x = (Vertices[0].x + Vertices[1].x + Vertices[2].x) / 3;
                var y = (Vertices[0].y + Vertices[1].y + Vertices[2].y) / 3;
                var z = (Vertices[0].z + Vertices[1].z + Vertices[2].z) / 3;
                return new Vector3(x, y, z);
            }
        }

        // This triangle's current transformation matrix.
        public Matrix4x4 Matrix;

        // The direction in which this triangle will move.
        public Vector3 Dir;

        // The rotation axis for this triangle.
        public Vector3 Rotation;

        // The UV coordinates for this triangle.
        public Vector3[] UV;

        // The vertices for this triangle.
        public Vector3[] Vertices;

        // The speed at which this triangle will move.
        public float Speed;

        // Disabled functionality for edge shading.
        //public Vector3[] BC;
    }
}