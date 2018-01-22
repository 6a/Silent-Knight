using UnityEngine;

namespace SilentKnight.PathFinding
{
    /// <summary>
    /// Container representing a line perpendicular to a point on a path. Used for smoothing path navigation.
    /// </summary>
    public struct Line
    {
        // Magic const!
        private const float VERTICAL_LINE_GRADIENT = 1e5f;

        // Line properties.
        float Gradient { get; set; }
        float YIntercept { get; set; }
        float GradientPerpendicular { get; set; }

        // Line data.
        Vector2 PointOnLine1;
        Vector2 PointOnLine2;
        bool ApproachSide;

        public Line(Vector2 pointOnLine, Vector2 pointPerpendicularToLine)
        {
            float deltaX = pointOnLine.x - pointPerpendicularToLine.x;
            float deltaY = pointOnLine.y - pointPerpendicularToLine.y;

            if (deltaX == 0)
            {
                GradientPerpendicular = VERTICAL_LINE_GRADIENT;
            }
            else
            {
                GradientPerpendicular = deltaY / deltaX;
            }

            if (GradientPerpendicular == 0)
            {
                Gradient = VERTICAL_LINE_GRADIENT;
            }
            else
            {
                Gradient = -1 / GradientPerpendicular;
            }

            YIntercept = pointOnLine.y - Gradient * pointOnLine.x;
            PointOnLine1 = pointOnLine;
            PointOnLine2 = pointOnLine + new Vector2(1, Gradient);

            ApproachSide = false;
            ApproachSide = GetSide(pointPerpendicularToLine);
        }

        /// <summary>
        /// Returns whether the point is over or before the current line.
        /// </summary>
        bool GetSide(Vector2 p)
        {
            return (p.x - PointOnLine1.x) * (PointOnLine2.y - PointOnLine1.y) > (p.y - PointOnLine1.y) * (PointOnLine2.x - PointOnLine1.x);
        }

        /// <summary>
        /// Returns true if this line has been crossed.
        /// </summary>
        public bool HasCrossedLine (Vector2 p)
        {
            return GetSide(p) != ApproachSide;
        }

        /// <summary>
        /// Returns distance between a point, and this line.
        /// </summary>
        public float DistanceFrom(Vector2 p)
        {
            var yInterceptPerpendicular = p.y - GradientPerpendicular * p.x;
            var intersectX = (yInterceptPerpendicular - YIntercept) / (Gradient - GradientPerpendicular);

            var intersectY = Gradient * intersectX + YIntercept;
            return Vector2.Distance(p, new Vector2(intersectX, intersectY));
        }
    }
}