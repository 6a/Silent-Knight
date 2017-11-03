using System;
using Entities;
using UnityEngine;

public class Chest : MonoBehaviour, ITargetable
{
    [SerializeField] Transform m_targetTransform;
    public Transform TargetTransform { get; set; }

    void Awake()
    {
        TargetTransform = m_targetTransform;
    }
}
