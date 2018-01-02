using System;

namespace PathFinding
{
    struct ASPath : IEquatable<ASPath>
    {
        public UnityEngine.Transform Seeker { get; private set; }
        public UnityEngine.Transform Target { get; private set; }

        public ASPath(UnityEngine.Transform seeker, UnityEngine.Transform target)
        {
            Seeker = seeker;
            Target = target;
        }

        public bool Equals(ASPath comparison)
        {
            if (Seeker == comparison.Seeker && Target == comparison.Target)
            {
                return true;
            }

            return false;
        }
    }
}
