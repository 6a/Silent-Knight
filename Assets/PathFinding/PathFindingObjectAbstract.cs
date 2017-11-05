using System.Collections;
using UnityEngine;

namespace PathFinding
{
    public abstract class PathFindingObject : MonoBehaviour
    {
        public abstract float Speed { get; set; }
        public abstract float TurnRate { get; set; }

        public Vector2[] Path { get; set; }

        int m_currentPathIndex = 1;

        public void SetNewTarget(Transform newTarget)
        {
            PathRequestManager.RequestPath(transform, newTarget, UpdatePath);
        }

        public void UpdatePath(Vector2[] newPath, bool success)
        {
            if (success) Path = newPath;
            StopFollowingPath();
            StartCoroutine(FollowPath());
        }

        public void StopFollowingPath()
        {
            StopCoroutine(FollowPath());
        }

        public void OnDrawGizmos()
        {
            if (Path != null)
            {
                for (int i = 0; i < Path.Length; i++)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(new Vector3(Path[i].x, 1, Path[i].y), 0.25f);

                    if (i == 0) continue;

                    if (i == m_currentPathIndex)
                    {
                        Gizmos.color = Color.black;
                        Gizmos.DrawLine(new Vector3(Path[m_currentPathIndex].x, 1, Path[m_currentPathIndex].y), new Vector3(Path[m_currentPathIndex - 1].x, 1, Path[m_currentPathIndex - 1].y));
                    }
                    else
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawLine(new Vector3(Path[i].x, 1, Path[i].y), new Vector3(Path[i - 1].x, 1, Path[i - 1].y));
                    }

                }
            }
        }

        public abstract IEnumerator FollowPath();

        /// <summary>
        /// Add actions in this functions for this unit to perform while moving, such as animations.
        /// </summary>
        public abstract void OnFollowPath();
    }
}