using UnityEngine;

namespace PathFinding
{
    public struct Line
    {
        private const float VERTICAL_LINE_GRADIENT = 1e5f;

        private float m_gradient { get; set; }
        private float m_yInterceipt { get; set; }
        private float m_gradientPerpendicular { get; set; }
        private Vector2 m_pointoOnLine1;
        private Vector2 m_pointOnLine2;
        private bool m_approachSide;

        public Line(Vector2 pointOnLine, Vector2 pointPerpendicularToLine)
        {
            float deltaX = pointOnLine.x - pointPerpendicularToLine.x;
            float deltaY = pointOnLine.y - pointPerpendicularToLine.y;

            if (deltaX == 0)
            {
                m_gradientPerpendicular = VERTICAL_LINE_GRADIENT;
            }
            else
            {
                m_gradientPerpendicular = deltaY / deltaX;
            }

            if (m_gradientPerpendicular == 0)
            {
                m_gradient = VERTICAL_LINE_GRADIENT;
            }
            else
            {
                m_gradient = -1 / m_gradientPerpendicular;
            }

            m_yInterceipt = pointOnLine.y - m_gradient * pointOnLine.x;
            m_pointoOnLine1 = pointOnLine;
            m_pointOnLine2 = pointOnLine + new Vector2(1, m_gradient);

            m_approachSide = false;
            m_approachSide = GetSide(pointPerpendicularToLine);
        }

        bool GetSide(Vector2 p)
        {
            return (p.x - m_pointoOnLine1.x) * (m_pointOnLine2.y - m_pointoOnLine1.y) > (p.y - m_pointoOnLine1.y) * (m_pointOnLine2.x - m_pointoOnLine1.x);
        }

        public bool HasCrossedLine (Vector2 p)
        {
            return GetSide(p) != m_approachSide;
        }

        public void Draw(float length)
        {
            var lineDirection = new Vector3(1, 0, m_gradient).normalized;
            var lineCentre = new Vector3(m_pointoOnLine1.x, 0, m_pointoOnLine1.y) + Vector3.up;
            Gizmos.DrawLine(lineCentre - lineDirection * length / 2f, lineCentre + lineDirection * length / 2f);
        }
    }
}