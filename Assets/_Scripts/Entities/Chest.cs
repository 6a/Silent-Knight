using System;
using Entities;
using UnityEngine;

/// <summary>
/// Convenience class for in-game chest objects.
/// </summary>
public class Chest : MonoBehaviour, ITargetable
{
    // Reference to the transform that will be considered as the target for this chest, for navigation purposes.
    [SerializeField] Transform m_targetTransform;

    /// <summary>
    /// Returns the target transform for this chest.
    /// </summary>
    Transform ITargetable.GetTransform()
    {
        return m_targetTransform;
    }
}
