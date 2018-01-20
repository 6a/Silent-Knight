using Entities;
using System.Collections;
using UnityEngine;

namespace PathFinding
{
    /// <summary>
    /// Abstract object for units that will use the AStar pathfinding tools in this project.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public abstract class PathFindingObject : MonoBehaviour, IEntity
    {
        // Debugging tools for path drawing.
        [SerializeField] Color m_pathColor;
        [SerializeField] bool m_drawPath;

        // Unit properties.
        public Path Path { get; set; }
        public ITargetable PathingTarget { get; set; }
        public int UnitID { get; set; }
        public bool HasReachedBoss { get; set; }
        public bool IsChangingView { get; set; }
        public bool Running { get; set; }
        public bool IsDead { get; set; }
        
        // Unit properties (that will be displayed in the editor on the derived object).
        public float Speed;
        public float TurnDistance;
        public float TurnSpeed;
        public float PathfindingTickDurationMS;
        public float StoppingDistance;
        public bool IsFollowingPath;
        public int CurrentPlatformIndex;

        // Position last frame.
        Vector3 m_prevPos;

        // Pending movement/rotations
        Quaternion newRotation;
        Vector3 nextMovement;
        bool processMovementUpdate;

        // Indices of the current node, stored for updating grid to avoid collisions.
        public Vector3 PreviousPos;

        // Reference to the attached line renderer.
        public LineRenderer LineRenderer;

        // Current pathfinding coroutine, used to prevent overlapping requests.
        IEnumerator m_currentPathCoroutine;

        /// <summary>
        /// Update the target for this unit.
        /// </summary>
        public void UpdatePathTarget(ITargetable newTarget)
        {
            PathingTarget = newTarget;
        }

        /// <summary>
        /// Part of the callback procedude for pathfinding requests. This is called once
        /// the pathfinding operations has finished.
        /// </summary>
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

        /// <summary>
        /// Temporarily halts this unit and prevents it from pathfinding.
        /// </summary>
        public void StopFollowingPath()
        {
            if (m_currentPathCoroutine == null) return;
            IsFollowingPath = false;
            StopCoroutine(m_currentPathCoroutine);
            m_currentPathCoroutine = null;
        }

        /// <summary>
        /// Updates the current path using the current path target. Should run continously while
        /// this unit is valid and running.
        /// </summary>
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

        /// <summary>
        /// Calculates pathfinding behaviour such as movement updates, facing, and animations.
        /// </summary>
        IEnumerator FollowPath()
        {
            IsFollowingPath = true;

            int pathIndex = 0;

            // Used to control animations.
            float speedPercent = 1;

            while (true)
            {
                // Early exit, if there is no path, if the unit is not currently moving, or if
                // the camera is transitioning.
                if (Path == null || !IsFollowingPath || IsChangingView)
                {
                    OnFollowPath(0);

                    // Wait one frame to prevent bugginess
                    yield return new WaitForFixedUpdate();
                    continue;
                }

                // Get current position
                var pos2D = transform.position.ToVector2();

                // Finds the next path point if one has just been surpassed.
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

                //Calculate animation speed and movement/rotation values.
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

                // Pass speedPercent value into OnFollowPath() for behaviours such as animation blending.
                OnFollowPath(speedPercent);
                yield return new WaitForFixedUpdate();
            }
        }

        /// <summary>
        /// Update this unit's position on the AStar grid, to reduce pathfinding unit collisions.
        /// </summary>
        void UpdateGridPosition(Vector3 pos)
        {
            if (PreviousPos == pos) return;

            ASGrid.UpdateGrid(PreviousPos, pos);
            PreviousPos = pos;
        }

        /// <summary>
        /// Applies any pending transformations. Call this from derived class from within the FixedUpdate() function.
        /// </summary>
        public void UpdateMovement()
        {
            if (processMovementUpdate)
            {
                transform.rotation = newRotation;
                transform.Translate(nextMovement);
                processMovementUpdate = false;
            }
        }

        /// <summary>
        /// Halt movement for this unit.
        /// </summary>
        public void StopMovement()
        {
            IsFollowingPath = false;
            processMovementUpdate = false;
        }

        /// <summary>
        /// Actions to perform every frame, while following the path.
        /// </summary>
        public abstract void OnFollowPath(float speedPercent);

        /// <summary>
        /// Actions to perform on the start of a level.
        /// </summary>
        public abstract void OnStartLevel();

        /// <summary>
        /// Reset the unit to pre-start values.
        /// </summary>
        public void Reset()
        {
            Running = false;
            IsDead = false;
        }
    }
}