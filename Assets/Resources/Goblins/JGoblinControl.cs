using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathFinding;
using System;
using Entities;

public class JGoblinControl : PathFindingObject, IEntity, ITargetable, IAttackable, IAttacker
{
    // References to attached components.
    Animator m_animator;

    public bool Running { get; set; }

    public Transform TargetTransform
    {
        get
        {
            return transform;
        }

        set
        {
            TargetTransform = value;
        }
    }

    public int Health { get; set; }

    public int DeathTime { get; set; }
    public bool IsDead { get; set; }

    public IAttackable CurrentTarget { get; set; }

    void Awake ()
    {
        m_animator = GetComponent<Animator>();
        LineRender = GetComponent<LineRenderer>();
        GameManager.OnStartRun += OnStartRun;
    }

	void Update ()
    {
		
	}

    public void Reset()
    {
        Running = false;
    }

    public override void OnEndRun()
    {

    }

    public override void OnFollowPath(float speedPercent)
    {
        if (speedPercent > 0) speedPercent = Mathf.Clamp01(speedPercent + 0.5f);

        m_animator.SetFloat("MovementBlend", 1 - speedPercent);
    }

    public override void OnStartRun()
    {
        PreviousPos = transform.position;

        CurrentTarget = FindObjectOfType<JKnightControl>();

        PathingTarget = (ITargetable)FindObjectOfType<JKnightControl>();

        UpdatePathTarget(PathingTarget);
        StartCoroutine(RefreshPath());
    }

    public void Damage()
    {
        throw new NotImplementedException();
    }

    public void KnockBack()
    {
        throw new NotImplementedException();
    }

    public void AfflictStatus(STATUS status)
    {
        throw new NotImplementedException();
    }

    public void Attack(IAttackable target)
    {
        throw new NotImplementedException();
    }

    public void GetInRange(IAttackable target)
    {
        throw new NotImplementedException();
    }

    public void AfflictStatus(IAttackable target)
    {
        throw new NotImplementedException();
    }

    Transform ITargetable.TargetTransform(int unitID)
    {
        return transform;
    }
}
