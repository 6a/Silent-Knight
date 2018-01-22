using System.Collections.Generic;
using UnityEngine;

namespace SilentKnight.PathFinding
{
    /// <summary>
    /// Smooths paths. 
    /// </summary>
    /// SOURCE: CraigSelbert @ codeproject.com - https://www.codeproject.com/Articles/18936/A-Csharp-Implementation-of-Douglas-Peucker-Line-Ap
    class PathSmoother
    {
        /// <summary>
        /// Uses the Douglas Peucker algorithm to reduce the number of points.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns></returns>
        public static List<Vector2> Reduce (List<Vector2> points, double tolerance)
        {
            if (points == null || points.Count < 3)
                return points;

            int firstPoint = 0;
            int lastPoint = points.Count - 1;
            List<int> indexedToKeep = new List<int>
            {
                //Add the first and last index to the keepers
                firstPoint,
                lastPoint
            };

            //The first and the last point cannot be the same
            while (points[firstPoint].Equals(points[lastPoint]))
            {
                lastPoint--;
            }

            DouglasPeuckerReduction(points, firstPoint, lastPoint,
            tolerance, ref indexedToKeep);

            List<Vector2> returnPoints = new List<Vector2>();
            indexedToKeep.Sort();
            foreach (int index in indexedToKeep)
            {
                returnPoints.Add(points[index]);
            }

            return returnPoints;
        }

        /// <summary>
        /// Douglases the peucker reduction.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="firstPoint">The first Vpointsector2.</param>
        /// <param name="lastPoint">The last points.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <param name="pointIndexsToKeep">The Vepointsctor2 index to keep.</param>
        private static void DouglasPeuckerReduction(List<Vector2> points, int firstPoint, int lastPoint, double tolerance, ref List<int> pointIndexsToKeep)
        {
            double maxDistance = 0;
            int indexFarthest = 0;

            for (int index = firstPoint; index < lastPoint; index++)
            {
                double distance = PerpendicularDistance
                    (points[firstPoint], points[lastPoint], points[index]);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    indexFarthest = index;
                }
            }

            if (maxDistance > tolerance && indexFarthest != 0)
            {
                //Add the largest point that exceeds the tolerance
                pointIndexsToKeep.Add(indexFarthest);

                DouglasPeuckerReduction(points, firstPoint,
                indexFarthest, tolerance, ref pointIndexsToKeep);
                DouglasPeuckerReduction(points, indexFarthest,
                lastPoint, tolerance, ref pointIndexsToKeep);
            }
        }

        /// <summary>
        /// The distance of a point from a line made from point1 and point.
        /// </summary>
        /// <param name="pt1">The PT1.</param>
        /// <param name="pt2">The PT2.</param>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        public static double PerpendicularDistance(Vector2 point1, Vector2 point2, Vector2 origin)
        {
            float area = Mathf.Abs(0.5f * (point1.x * point2.y + point2.x *
            origin.y + origin.x * point1.y - point2.x * point1.y - origin.x *
            point2.y - point1.x * origin.y));
            double bottom = Mathf.Sqrt(Mathf.Pow(point1.x - point2.x, 2) +
            Mathf.Pow(point1.y - point2.y, 2));
            double height = area / bottom * 2;

            return height;
        }
    }
}
