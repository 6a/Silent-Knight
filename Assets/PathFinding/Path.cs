using UnityEngine;

namespace PathFinding
{
    public class Path
    {
        public readonly Vector2[] LookPoints;
        public readonly Line[] TurnBoundaries;
        public readonly int FinishLineIndex;
        public readonly int SlowdownIndex;

        public Path(Vector2[] waypoints, Vector3 startPos, float turnDistance, float stoppingDistance)
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

        public void Draw(LineRenderer lr)
        {
            lr.widthMultiplier = 0.2f;
            lr.material = new Material(Shader.Find("Unlit/Color"));
            lr.material.color = Color.cyan;

            lr.positionCount = LookPoints.Length;
            for (int i = 0; i < LookPoints.Length; i++)
            {
                lr.SetPosition(i, new Vector3(LookPoints[i].x, 1, LookPoints[i].y));
            }
        }

    }
}
