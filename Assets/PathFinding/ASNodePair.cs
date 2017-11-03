using System;

namespace PathFinding
{
    struct ASNodePair : System.IEquatable<ASNodePair>
    {
        public int Seeker { get; set; }
        public int Target { get; set; }

        public ASNodePair(int seekerID, int targetID)
        {
            Seeker = seekerID;
            Target = targetID;
        }

        public bool Equals(ASNodePair comparison)
        {
            if (Seeker == comparison.Seeker && Target == comparison.Target) return true;
        
            return false;
        }
    }
}
