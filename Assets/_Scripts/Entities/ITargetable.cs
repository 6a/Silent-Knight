using UnityEngine;

namespace Entities
{
    /// <summary>
    /// A unit that can be targeted.
    /// </summary>
    public interface ITargetable
    {
        // Returns the target transform for this unit.
        Transform GetTransform();
    }
}
