using UnityEngine;

namespace Entities
{
    /// <summary>
    /// A units that can be targeted
    /// </summary>
    public interface ITargetable
    {
        Transform GetTransform();
    }
}
