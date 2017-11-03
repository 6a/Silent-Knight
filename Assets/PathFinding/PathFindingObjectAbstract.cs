using System.Collections.Generic;

namespace PathFinding
{
    public abstract class PathFindingObject : UnityEngine.MonoBehaviour
    {
        public int? PathFinderID = null;
        public List<ASNode> Path { get; set; }

        public void RegisterPathID(ASPathFinder pathFinder, UnityEngine.Transform target)
        {
            if (!PathFinderID.HasValue)
            {
                PathFinderID = pathFinder.Register(transform, target);
            }
            else
            {
                print("This object is already registered to the pathfinder with ID [" + PathFinderID + "]");
            }
        }

        public void SetNewTarget(ASPathFinder pathFinder, UnityEngine.Transform newTarget)
        {
            if (PathFinderID.HasValue)
            {
                pathFinder.UpdatePathTarget(PathFinderID.Value, newTarget);
            }
            else
            {
                print("This object is not yet registered to the pathfinder");
            }
        }

        public void UnRegisterPathID(ASPathFinder pathFinder)
        {
            if (PathFinderID.HasValue)
            {
                pathFinder.UnRegister(PathFinderID.Value);
                PathFinderID = null;
            }
            else
            {
                print("This object is not yet registered to the pathfinder");
            }
        }

        public void UpdatePath(ASPathFinder pathFinder)
        {
            Path = pathFinder.GetPath(PathFinderID.Value);
        }
    }
}