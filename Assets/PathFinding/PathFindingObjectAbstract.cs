using System.Collections;
using UnityEngine;

namespace PathFinding
{
    public abstract class PathFindingObject : MonoBehaviour
    {
        public float Speed;
        public float TurnDistance;
        public float TurnSpeed;
        public float PathUpdateMoveThreshold;
        public float PathfindingTickDurationMS;

        public bool IsFollowingPath;

        private Transform m_target;
        private IEnumerator m_currentPathCoroutine;
        private bool m_newPath;

        public Path Path { get; set; }

        public void UpdatePathTarget(Transform newTarget)
        {
            m_target = newTarget;
            m_newPath = true;
        }

        public void OnPathFound(Vector2[] wayPoints, bool success)
        {
            if (success)
            {
                StopFollowingPath();
                Path = new Path(wayPoints, transform.position, TurnDistance);
                if (m_currentPathCoroutine == null) m_currentPathCoroutine = FollowPath();
                StartCoroutine(m_currentPathCoroutine);
            }
        }

        public void StopFollowingPath()
        {
            if (m_currentPathCoroutine == null) return;
            IsFollowingPath = false;
            StopCoroutine(m_currentPathCoroutine);
            m_currentPathCoroutine = null;
        }

        public void OnDrawGizmos()
        {
            if (Path != null)
            {
                Path.Draw();
            }
        }

        public IEnumerator RefreshPath()
        {
            var sqrMoveThreshold = PathUpdateMoveThreshold * PathUpdateMoveThreshold;
            var targetPosOld = m_target.position;

            while (true)
            {
                yield return new WaitForSeconds(PathfindingTickDurationMS / 1000f);
                if (m_newPath || (m_target.position - targetPosOld).sqrMagnitude < sqrMoveThreshold)
                {
                    m_newPath = false;
                    PathRequestManager.RequestPath(transform, m_target, OnPathFound);
                    targetPosOld = m_target.position;
                }
            }
        }

        public IEnumerator FollowPath()
        {
            IsFollowingPath = true;
            int pathIndex = 0;

            while (IsFollowingPath)
            {
                var pos2D = new Vector2(transform.position.x, transform.position.z);

                while (Path.TurnBoundaries[pathIndex].HasCrossedLine(pos2D))
                {
                    if (pathIndex >= Path.FinishLineIndex)
                    {
                        IsFollowingPath = false;
                        break;
                    }
                    else
                    {
                        pathIndex++;
                    }
                }

                if (IsFollowingPath)
                {
                    var targetRotation = Quaternion.LookRotation(new Vector3(Path.LookPoints[pathIndex].x, transform.position.y, Path.LookPoints[pathIndex].y) - transform.position);
                    newRotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * TurnSpeed);
                    nextMovement = (Vector3.forward * Time.deltaTime * Speed);

                    processMovementUpdate = true;
                }

                OnFollowPath();
                yield return null;
            }
        }

        Quaternion newRotation;
        Vector3 nextMovement;
        bool processMovementUpdate;

        void LateUpdate()
        {
            if (!processMovementUpdate) return;
            transform.rotation = newRotation;
            transform.Translate(nextMovement);
            processMovementUpdate = false;

        }

        /// <summary>
        /// Add actions in this functions for this unit to perform while moving, such as animations.
        /// </summary>
        public abstract void OnFollowPath();
    }
}