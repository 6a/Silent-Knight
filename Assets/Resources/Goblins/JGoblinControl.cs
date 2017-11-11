using UnityEngine;
using PathFinding;
using System;
using Entities;
using System.Collections;

public class JGoblinControl : PathFindingObject, IEntity, ITargetable, IAttackable, IAttacker
{
    // References to attached components.
    Animator m_animator;
    Rigidbody m_rb;

    public bool Running { get; set; }

    public float Health { get; set; }

    public int DeathTime { get; set; }
    public bool IsDead { get; set; }

    public IAttackable CurrentTarget { get; set; }

    [SerializeField] float m_attackRange;
    [SerializeField] float m_attacksPerSecond;
    [SerializeField] float m_baseDamage;
    [SerializeField] float m_level;
    [SerializeField] float m_baseHealth;

    float m_lastAttackTime;

    public bool Active { get; set; }

    void Awake ()
    {
        m_animator = GetComponent<Animator>();
        LineRender = GetComponent<LineRenderer>();
        m_rb = GetComponent<Rigidbody>();
        Active = true; //TODO revert
        m_lastAttackTime = -1;
        Health = (m_baseHealth * (1 + (m_level - 1) * LevelMultipliers.HEALTH));
        GameManager.OnStartRun += OnStartRun;
    }

	void Update ()
    {
        if (!Active) return;

        Attack(CurrentTarget);
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

        PathingTarget = FindObjectOfType<JKnightControl>();

        GetInRange(PathingTarget);
    }

    public void Damage(float dmg)
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
        var distanceToTarget = Vector3.Distance(transform.position, target.Position());

        if (distanceToTarget <= m_attackRange)
        {
            transform.LookAt(target.Position());

            float delay = 1000f / (1000f * m_attacksPerSecond);

            if (m_lastAttackTime == -1 || Time.time - m_lastAttackTime > delay)
            {
                InterruptAnimator();

                int rand = UnityEngine.Random.Range(0, 30);

                if (rand > 9)
                {
                    TriggerAnimation(ANIMATION.ATTACK_BASIC);
                    StartCoroutine(ApplyDamageDelayed(1, 7, target));
                }
                else if (rand > 2)
                {
                    TriggerAnimation(ANIMATION.ATTACK_SPECIAL);
                    StartCoroutine(ApplyDamageDelayed(1, 7, target));
                    StartCoroutine(ApplyDamageDelayed(1, 19, target));
                }
                else
                {
                    TriggerAnimation(ANIMATION.ATTACK_ULTIMATE);
                    StartCoroutine(ApplyDamageDelayed(5, 16, target));
                }

                m_lastAttackTime = Time.time;
            }
        }
    }

    public void GetInRange(ITargetable target)
    {
        UpdatePathTarget(PathingTarget);
        StartCoroutine(RefreshPath());
    }

    public void AfflictStatus(IAttackable target)
    {
        throw new NotImplementedException();
    }

    Transform ITargetable.TargetTransform(int unitID)
    {
        return transform;
    }

    public Vector3 Position()
    {
        return transform.position;
    }

    IEnumerator ApplyDamageDelayed(int dmgMultiplier, int frameDelay, IAttackable target)
    {
        yield return new WaitForSeconds(Time.fixedDeltaTime * frameDelay);

        target.Damage(dmgMultiplier * m_baseDamage * (1 + (m_level - 1 ) * LevelMultipliers.DAMAGE));
    }

    void TriggerAnimation(ANIMATION anim)
    {
        switch (anim)
        {
            case ANIMATION.ATTACK_BASIC:
                InterruptAnimator();
                m_animator.SetTrigger("A1Start");
                break;
            case ANIMATION.ATTACK_SPECIAL:
                InterruptAnimator();
                m_animator.SetTrigger("A2Start");
                break;
            case ANIMATION.ATTACK_ULTIMATE:
                InterruptAnimator();
                m_animator.SetTrigger("A3Start");
                break;
            case ANIMATION.DEATH:
                InterruptAnimator();
                m_animator.SetTrigger("DeathStart");
                break;
        }
    }

    void InterruptAnimator()
    {
        m_animator.SetTrigger("Interrupt");

        m_animator.ResetTrigger("A1Start");
        m_animator.ResetTrigger("A2Start");
        m_animator.ResetTrigger("A3Start");
        m_animator.ResetTrigger("DeathStart");
    }
}
