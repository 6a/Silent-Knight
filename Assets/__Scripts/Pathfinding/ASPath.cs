using UnityEngine;
using SilentKnight.Utility;

namespace SilentKnight.PathFinding
{
    /// <summary>
    /// Container representing a path (a set of waypoints) as well as various tools for smooth pathing.
    /// </summary>
    public class ASPath
    {
        // Path properties.
        public Vector2[] LookPoints     { get; private set; }
        public Line[] TurnBoundaries    { get; private set; }
        public int FinishLineIndex      { get; private set; }
        public int SlowdownIndex        { get; private set; }

        public ASPath(Vector2[] waypoints, Vector3 startPos, float turnDistance, float stoppingDistance)
        {
            // Loads waypoints into memory and sets up all turn boundaries and lookpoints.

            LookPoints = waypoints;
            TurnBoundaries = new Line[LookPoints.Length];
            FinishLineIndex = TurnBoundaries.Length - 1;

            var previousPoint = startPos.ToVector2();
            for (int i = 0; i < LookPoints.Length; i++)
            {
                var currentPoint = LookPoints[i];
                var dirToCurrentPoint = (currentPoint - previousPoint).normalized;
                var turnBoundaryPoint = (i == FinishLineIndex) ? currentPoint : currentPoint - dirToCurrentPoint * turnDistance;
                TurnBoundaries[i] = new Line(turnBoundaryPoint, previousPoint - dirToCurrentPoint * turnDistance);
                previousPoint = turnBoundaryPoint;
            }

            var distanceFromEndPoint = 0f;

            for (int i = LookPoints.Length - 1; i > 0; i--)
            {
                distanceFromEndPoint += Vector3.Distance(LookPoints[i], LookPoints[i - 1]);
                if (distanceFromEndPoint > stoppingDistance)
                {
                    SlowdownIndex = i;
                    break;
                }
            }
        }

        /// <summary>
        /// Draws the path in-game, for debugging purposes.
        /// </summary>
        public void Draw(LineRenderer lr, Color color)
        {
            lr.widthMultiplier = 0.2f;
            lr.material = new Material(Shader.Find("Unlit/Color")) { color = color };
            lr.positionCount = LookPoints.Length;

            for (int i = 0; i < LookPoints.Length; i++)
            {
                lr.SetPosition(i, new Vector3(LookPoints[i].x, 1.1f, LookPoints[i].y));
            }
        }
    }
}
