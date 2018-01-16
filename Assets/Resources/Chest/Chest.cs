using System;
using Entities;
using UnityEngine;

public class Chest : MonoBehaviour, ITargetable
{
    [SerializeField] Transform m_targetTransform;

    Transform ITargetable.GetTransform()
    {
        return m_targetTransform;
    }
}
