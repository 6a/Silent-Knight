using UnityEngine;
using PathFinding;
using System;
using Entities;
using System.Collections;

public class JGoblinControl : PathFindingObject, ITargetable, IAttackable, IAttacker
{
    // References to attached components.
    Animator m_animator;
    Rigidbody m_rb;

    public float Health { get; set; }

    public int DeathTime { get; set; }

    public IAttackable CurrentTarget { get; set; }

    [SerializeField] float m_attackRange;
    [SerializeField] float m_attacksPerSecond;
    [SerializeField] float m_baseDamage;
    [SerializeField] float m_level;
    [SerializeField] float m_baseHealth;

    float m_lastAttackTime;

    void Awake ()
    {
        m_animator = GetComponent<Animator>();
        LineRender = GetComponent<LineRenderer>();
        m_rb = GetComponent<Rigidbody>();
        m_lastAttackTime = -1;
        Health = (m_baseHealth * (1 + (m_level - 1) * LevelMultipliers.HEALTH));
        GameManager.OnStartRun += OnStartRun;
    }

	void Update ()
    {
        if (!Running) return;

        Attack(CurrentTarget);
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

        StartCoroutine(RefreshPath());

        GetInRange(PathingTarget);
    }

    public void Damage(IAttacker attacker, float dmg)
    {
        if (IsDead) return;

        Health -= Mathf.Max(dmg, 0);

        //print(dmg + " damage received. " + Health + " health remaining.");

        if (Health <= 0)
        {
            print("Goblin died");
            Running = false;
            InterruptAnimator();
            TriggerAnimation(ANIMATION.DEATH);
            IsDead = true;
            attacker.OnTargetDied(this);
        }
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
            StopMovement();

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

        target.Damage(this, dmgMultiplier * m_baseDamage * (1 + (m_level - 1 ) * LevelMultipliers.DAMAGE));
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
                // Particles or stuff
                AI.RemoveUnit(Platforms.PlayerPlatform, this);

                transform.position = new Vector3(0, -1000, 0);

                Disposal.Dispose(gameObject);

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

    public void OnTargetDied(IAttackable target)
    {
        throw new NotImplementedException();
    }

    public ITargetable GetTargetableInterface()
    {
        return this;
    }
}
