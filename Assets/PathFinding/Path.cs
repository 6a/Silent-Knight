using UnityEngine;

namespace PathFinding
{
    public class Path
    {
        public readonly Vector2[] LookPoints;
        public readonly Line[] TurnBoundaries;
        public readonly int FinishLineIndex;

        public Path(Vector2[] waypoints, Vector3 startPos, float turnDistance)
        {
            LookPoints = waypoints;
            TurnBoundaries = new Line[LookPoints.Length];
            FinishLineIndex = TurnBoundaries.Length - 1;

            var previousPoint = startPos.Reduce();
            for (int i = 0; i < LookPoints.Length; i++)
            {
                var currentPoint = LookPoints[i];
                var dirToCurrentPoint = (currentPoint - previousPoint).normalized;
                var turnBoundaryPoint = (i == FinishLineIndex) ? currentPoint : currentPoint - dirToCurrentPoint * turnDistance;
                TurnBoundaries[i] = new Line(turnBoundaryPoint, previousPoint - dirToCurrentPoint * turnDistance);
                previousPoint = turnBoundaryPoint;
            }
        }

        public void Draw()
        {
            Gizmos.color = Color.red;
            foreach (var p in LookPoints)
            {
                Gizmos.DrawSphere(new Vector3(p.x, 1, p.y), 1);
            }

            Gizmos.color = Color.white;
            foreach (var l in TurnBoundaries)
            {
                l.Draw(10);
            }
        }

    }
}
