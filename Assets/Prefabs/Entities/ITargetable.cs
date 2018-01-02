using UnityEngine;

namespace Entities
{
    public interface ITargetable
    {
        Transform TargetTransform(int unitID);
    }
}
