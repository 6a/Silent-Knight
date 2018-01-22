using System;
using UnityEngine;

namespace SilentKnight.PathFinding
{
    /// <summary>
    /// Container for pathfinding requests.
    /// </summary>
    public struct PathRequest
    {
        // Start and destination. Transforms are used in as these will track any changes to their parent objects.
        // Vectors become inaccurate the moment that the parent objects are manipulated.
        public Transform Start { get; set; }
        public Transform End { get; set; }

        // Callback to call once pathfinding has been completed.
        public Action<Vector2[], bool> Callback { get; set; }

        public PathRequest(Transform start, Transform end, Action<Vector2[], bool> callback)
        {
            Start = start;
            End = end;
            Callback = callback;
        }
    }
}
