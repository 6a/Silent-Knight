using System;
using UnityEngine;

namespace SilentKnight.PathFinding
{
    /// <summary>
    /// Represents the result of a pathfinding search, including the path itself.
    /// </summary>
    public struct PathResult
    {
        public Vector2[] Path { get; set; }
        public bool Success { get; set; }
        public Action<Vector2[], bool> Callback { get; set; }

        public PathResult(Vector2[] path, bool success, Action<Vector2[], bool> callback)
        {
            Path = path;
            Success = success;
            Callback = callback;
        }
    }
}
