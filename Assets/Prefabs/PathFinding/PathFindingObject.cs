using Entities;
using System.Collections;
using UnityEngine;
using System;

namespace PathFinding
{
    [RequireComponent(typeof(LineRenderer))]
    public abstract class PathFindingObject : MonoBehaviour, IEntity
    {
        [SerializeField] Color m_pathColor;
        [SerializeField] bool m_drawPath;

        public Path Path { get; set; }
        public ITargetable PathingTarget { get; set; }
        public int UnitID { get; set; }
        public bool HasReachedBoss { get; set; }
        public bool IsChangingView { get; set; }

        public bool Running { get; set; }

        public bool IsDead { get; set; }

        public float Speed;
        public float TurnDistance;
        public float TurnSpeed;
        public float PathfindingTickDurationMS;
        public float StoppingDistance;
        public bool IsFollowingPath;
        public int CurrentPlatformIndex;

        // Indices of the current node, stored for updating grid to avoid collisions.
        public Vector3 PreviousPos;

        public LineRenderer LineRenderer;

        IEnumerator m_currentPathCoroutine;

        public void UpdatePathTarget(ITargetable newTarget)
        {
            PathingTarget = newTarget;
        }

        public void OnPathFound(Vector2[] wayPoints, bool success)
        {
            if (Running && success)
            {
                StopFollowingPath();
                Path = new Path(wayPoints, transform.position, TurnDistance, StoppingDistance);
                if (m_currentPathCoroutine == null) m_currentPathCoroutine = FollowPath();
                StartCoroutine(m_currentPathCoroutine);

                if (Path != null && m_drawPath)
                {
                    Path.Draw(LineRenderer, m_pathColor);
                }
            }
            else
            {
                Path = null;
            }
        }

        public void StopFollowingPath()
        {
            if (m_currentPathCoroutine == null) return;
            IsFollowingPath = false;
            StopCoroutine(m_currentPathCoroutine);
            m_currentPathCoroutine = null;
        }

        Vector3 m_prevPos;

        public IEnumerator RefreshPath()
        {
            while (true)
            {
                yield return new WaitForSeconds(PathfindingTickDurationMS / 1000f);

                if (!Running || transform == null) continue;

                if (m_prevPos != PathingTarget.GetTransform().position)
                {
                    PathRequestManager.RequestPath(new PathRequest(transform, PathingTarget.GetTransform(), OnPathFound));
                    m_prevPos = PathingTarget.GetTransform().position;
                }
            }
        }

        IEnumerator FollowPath()
        {
            IsFollowingPath = true;

            int pathIndex = 0;

            float speedPercent = 1;

            while (true)
            {
                if (Path == null || !IsFollowingPath || IsChangingView)
                {
                    OnFollowPath(0);
                    yield return new WaitForFixedUpdate();
                    continue;
                }

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

                if (IsFollowingPath && !HasReachedBoss)
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
                yield return new WaitForFixedUpdate();
            }
        }

        void UpdateGridPosition(Vector3 pos)
        {
            if (PreviousPos == pos) return;

            ASGrid.UpdateGrid(PreviousPos, pos);
            PreviousPos = pos;
        }

        Quaternion newRotation;
        Vector3 nextMovement;
        bool processMovementUpdate;

        // Applies any pending translation
        public void UpdateMovement()
        {
            if (processMovementUpdate)
            {
                transform.rotation = newRotation;
                transform.Translate(nextMovement);
                processMovementUpdate = false;
            }
        }

        public void StopMovement()
        {
            IsFollowingPath = false;
            processMovementUpdate = false;
        }

        /// <summary>
        /// Add actions in this functions for this unit to perform while moving, such as animations.
        /// </summary>
        public abstract void OnFollowPath(float speedPercent);

        public abstract void OnStartRun();

        public void Reset()
        {
            Running = false;
            IsDead = false;
        }
    }
}