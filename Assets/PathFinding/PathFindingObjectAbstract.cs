using System.Collections;
using UnityEngine;

namespace PathFinding
{
    [RequireComponent(typeof(LineRenderer))]
    public abstract class PathFindingObject : MonoBehaviour
    {
        [SerializeField] Color m_pathColor;
        [SerializeField] bool m_drawPath;

        public Path Path { get; set; }

        public float Speed;
        public float TurnDistance;
        public float TurnSpeed;
        public float PathUpdateMoveThreshold;
        public float PathfindingTickDurationMS;
        public float StoppingDistance;
        public bool IsFollowingPath;

        public LineRenderer LineRender;

        Transform m_target;
        IEnumerator m_currentPathCoroutine;
        bool m_newPath;

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
                Path = new Path(wayPoints, transform.position, TurnDistance, StoppingDistance);
                if (m_currentPathCoroutine == null) m_currentPathCoroutine = FollowPath();
                StartCoroutine(m_currentPathCoroutine);

                if (Path != null && m_drawPath)
                {
                    Path.Draw(LineRender, m_pathColor);
                }
            }
        }

        public void StopFollowingPath()
        {
            if (m_currentPathCoroutine == null) return;
            IsFollowingPath = false;
            StopCoroutine(m_currentPathCoroutine);
            m_currentPathCoroutine = null;
        }

        public IEnumerator RefreshPath()
        {
            var sqrMoveThreshold = PathUpdateMoveThreshold * PathUpdateMoveThreshold;
            var targetPosOld = m_target.position;

            while (true)
            {
                yield return new WaitForSeconds(PathfindingTickDurationMS / 1000f);
                if (m_newPath || (m_target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold)
                {
                    m_newPath = false;
                    PathRequestManager.RequestPath(new PathRequest(transform, m_target, OnPathFound));
                    print("New path requested");
                    targetPosOld = m_target.position;
                }
            }
        }

        IEnumerator FollowPath()
        {
            IsFollowingPath = true;
            int pathIndex = 0;

            float speedPercent = 1;

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
                    if (pathIndex >= Path.SlowdownIndex && StoppingDistance > 0)
                    {
                        speedPercent = Mathf.Clamp01(Path.TurnBoundaries[Path.FinishLineIndex].DistanceFrom(pos2D) / StoppingDistance * 1.5f);
                        if (speedPercent <= 0.1f)
                        {
                            speedPercent = 0;
                            IsFollowingPath = false;
                        }
                    }

                    var targetRotation = Quaternion.LookRotation(new Vector3(Path.LookPoints[pathIndex].x, transform.position.y, Path.LookPoints[pathIndex].y) - transform.position);
                    newRotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * TurnSpeed);
                    nextMovement = (Vector3.forward * Time.deltaTime * Speed * speedPercent);

                    processMovementUpdate = true;
                }

                OnFollowPath(speedPercent);
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
        public abstract void OnFollowPath(float speedPercent);

        public abstract void OnStartRun();
        public abstract void OnEndRun();
    }
}