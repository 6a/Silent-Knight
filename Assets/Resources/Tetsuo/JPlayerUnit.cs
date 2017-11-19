using System;
using PathFinding;
using UnityEngine;
using Entities;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles control behaviour for player character.
/// </summary>
public class JPlayerUnit : PathFindingObject, IAttackable, IAttacker, ITargetable
{
    enum SPECIAL { ULTIMATE, KICK, SHIELD }

    // Variables exposed in the editor.
    [SerializeField] float m_horizontalMod, m_linearMod, m_jumpForce, m_groundTriggerDistance;

    // References to attached components.
    Animator m_animator;
    Rigidbody m_rb;
    PlayerWeapon m_weapon;

    // Public property used to check knight focus point
    public Vector3 FocusPoint
    {
        get { return transform.position; }
        set { FocusPoint = value; }
    }

    public float Health { get; set; }

    public int DeathTime { get; set; }

    public IAttackable CurrentTarget { get; set; }

    public int ID { get; set; }

    [SerializeField] float m_attackRange;
    [SerializeField] float m_attacksPerSecond;
    [SerializeField] float m_baseDamage;
    [SerializeField] float m_level;
    [SerializeField] float m_baseHealth;

    float m_lastAttackTime;
    ITargetable m_chest;
    bool m_isDoingUltimate;
    bool m_isKicking;
    bool m_isDoingSpecial;
    bool m_isShieldAttacking;
    IEnumerator m_currentDamageCoroutine;
    int m_attackState;

    void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_rb = GetComponent<Rigidbody>();
        LineRender = GetComponent<LineRenderer>();
        m_weapon = GetComponentInChildren<PlayerWeapon>();
        m_lastAttackTime = -1;
        Health = (m_baseHealth * (1 + (m_level - 1) * LevelMultipliers.HEALTH));
        m_attackState = 0;
        m_currentDamageCoroutine = null;

        GameManager.OnStartRun += OnStartRun;
    }

    void Start()
    {
        DisableWCollider();
    }

    void Update()
    {
        if (!Running) return;

        Attack(CurrentTarget);
    }

    public override void OnFollowPath(float speedPercent)
    {
        if (speedPercent > 0) speedPercent = Mathf.Clamp01(speedPercent + 0.5f);

        m_animator.SetFloat("MovementBlend", 1 - speedPercent);
    }

    public void EnableWCollider()
    {
        m_weapon.Switch(true);
    }

    void DisableWCollider()
    {
        m_weapon.Switch(false);
    }

    // Triggers appropriate animation. Is set to interrupt the current animation, and then trigger
    // the appropriate one.
    private void TriggerAnimation(ANIMATION anim, bool interrupt = true)
    {
        if (interrupt) InterruptAnimator();

        switch (anim)
        {
            case ANIMATION.ATTACK_BASIC:
                m_animator.SetTrigger("A1Start");
                break;
            case ANIMATION.ATTACK_SPECIAL:
                m_animator.SetTrigger("A2Start");
                break;
            case ANIMATION.ATTACK_ULTIMATE:
                m_animator.SetTrigger("A3Start");
                break;
            case ANIMATION.ATTACK_KICK:
                m_animator.SetTrigger("KickStart");
                break;
            case ANIMATION.ATTACK_SHIELD:
                m_animator.SetTrigger("ShieldStart");
                break;
            case ANIMATION.PARRY:
                m_animator.SetTrigger("ParryStart");
                break;
            case ANIMATION.BUFF:
                m_animator.SetTrigger("BuffStart");
                break;
            case ANIMATION.DEATH:
                m_animator.SetTrigger("DeathStart");
                break;
            case ANIMATION.JUMP:
                m_animator.SetTrigger("JumpStart");
                break;
        }
    }

    void InterruptAnimator()
    {
        m_animator.SetTrigger("Interrupt");

        m_animator.ResetTrigger("A1Start");
        m_animator.ResetTrigger("A2Start");
        m_animator.ResetTrigger("A3Start");
        m_animator.ResetTrigger("KickStart");
        m_animator.ResetTrigger("ShieldStart");
        m_animator.ResetTrigger("ParryStart");
        m_animator.ResetTrigger("BuffStart");
        m_animator.ResetTrigger("DeathStart");
        m_animator.ResetTrigger("JumpStart");
    }

    List<int> enemiesHit = new List<int>();

    public void OnContactEnemy(IAttackable enemy)
    {
        if (enemy == null) return;

        if (m_isDoingUltimate && !enemiesHit.Contains(enemy.ID))
        {
            enemiesHit.Add(enemy.ID);
            enemy.Damage(this, m_baseDamage * 3);
            StartCoroutine(Freeze(0.1f));

            // TODO particles

            enemy.KnockBack(new Vector2(transform.position.x, transform.position.z), 400);
        }
    }

    public void OnShieldAttack()
    {
        if (CurrentTarget == null) return;

        CurrentTarget.Damage(this, m_baseDamage * 2);
        StartCoroutine(Freeze(0.1f));

        // TODO particles

        CurrentTarget.AfflictStatus(STATUS.STUN, 2);


        ToggleSpecial(SPECIAL.SHIELD, false);
        m_lastAttackTime = Time.time - 0.5f;
        OnFollowPath(0);
    }

    public void OnKick()
    {
        if (CurrentTarget == null) return;

        CurrentTarget.Damage(this, m_baseDamage * 2);
        StartCoroutine(Freeze(0.1f));

        // TODO particles

        CurrentTarget.KnockBack(new Vector2(transform.position.x, transform.position.z), 800);

        ToggleSpecial(SPECIAL.KICK, false);
        m_lastAttackTime = Time.time - 0.5f;
        OnFollowPath(0);
    }

    public void SpecialFinished()
    {
        DisableWCollider();
        ToggleSpecial(SPECIAL.ULTIMATE, false);
        m_lastAttackTime = Time.time - 0.5f;
        enemiesHit = new List<int>();
        OnFollowPath(0);
    }

    // Helper function that uses a raycast to check if the knight is touching a surface with their feet.
    bool IsAirborne()
    {
        if (Physics.Raycast(transform.position + (Vector3.up * 0.5f), Vector3.down, m_groundTriggerDistance))
        {
            m_animator.SetTrigger("JumpEnd");

            return false;
        }

        return true;
    }

    public void OnEnterPlatform()
    {
        IAttackable nextTarget;

        nextTarget = AI.GetNewTarget(transform.position, Platforms.PlayerPlatform);

        if (nextTarget == null)
        {
            PathingTarget = m_chest;
            UpdatePathTarget(PathingTarget);
            CurrentTarget = null;
        }
        else
        {
            CurrentTarget = nextTarget;
            GetInRange(nextTarget.GetTargetableInterface());
        }
    }

    public override void OnStartRun()
    {
        Platforms.RegisterPlayer(this);

        PreviousPos = transform.position;

        CurrentTarget = null;

        PathingTarget = FindObjectOfType<Chest>();

        m_chest = PathingTarget;

        Running = true;

        UpdatePathTarget(PathingTarget);
        StartCoroutine(RefreshPath());
    }

    public override void OnEndRun()
    {

    }

    public void Damage(IAttacker attacker, float dmg)
    {
        if (IsDead) return;

        Health -= Mathf.Max(dmg, 0);

        //print(dmg + " damage received. " + Health + " health remaining.");

        if (Health <= 0)
        {
            print("Player died");
            Running = false;
            TriggerAnimation(ANIMATION.DEATH);
            IsDead = true;
            attacker.OnTargetDied(this);
        }
    }

    public void KnockBack(Vector2 sourcePos, float strength)
    {
        throw new NotImplementedException();
    }

    public void AfflictStatus(STATUS status, float duration)
    {
        throw new NotImplementedException();
    }

    void ToggleSpecial(SPECIAL type, bool on)
    {
        m_isDoingSpecial = on;

        switch (type)
        {
            case SPECIAL.ULTIMATE:
                m_isDoingUltimate = on;
                break;
            case SPECIAL.KICK:
                m_isKicking = on;
                break;
            case SPECIAL.SHIELD:
                m_isShieldAttacking = on;
                break;
        }
    }

    public void Attack(IAttackable target)
    {
        if (target == null) return;

        float attackDelay = 1000f / (1000f * m_attacksPerSecond);

        var distanceToTarget = Vector3.Distance(transform.position, target.Position());
        if (distanceToTarget < m_attackRange)
        {
            StopMovement();
            OnFollowPath(0);

            var targetRotation = Quaternion.LookRotation(target.Position() - transform.position);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 4);

            if (!m_isDoingSpecial)
            {
                if (Input.GetButtonDown("AttackUltimate"))
                {
                    ToggleSpecial(SPECIAL.ULTIMATE, true);
                    if (m_currentDamageCoroutine != null) StopCoroutine(m_currentDamageCoroutine);
                    TriggerAnimation(ANIMATION.ATTACK_ULTIMATE);
                    return;
                }

                if (Input.GetButtonDown("AttackKick"))
                {
                    ToggleSpecial(SPECIAL.KICK, true);
                    if (m_currentDamageCoroutine != null) StopCoroutine(m_currentDamageCoroutine);
                    TriggerAnimation(ANIMATION.ATTACK_KICK);
                    return;
                }

                if (Input.GetButtonDown("AttackShield"))
                {
                    ToggleSpecial(SPECIAL.SHIELD, true);
                    if (m_currentDamageCoroutine != null) StopCoroutine(m_currentDamageCoroutine);
                    TriggerAnimation(ANIMATION.ATTACK_SHIELD);
                    return;
                }
            }
            
            if (!m_isDoingSpecial && (m_lastAttackTime == -1 || Time.time - m_lastAttackTime > attackDelay))
            {
                m_attackState = (m_attackState == 0) ? 1 : 0;

                if (m_attackState == 0)
                {
                    TriggerAnimation(ANIMATION.ATTACK_BASIC);
                    m_currentDamageCoroutine = ApplyDamageDelayed(1, 30, target);
                    StartCoroutine(m_currentDamageCoroutine);
                }
                else
                {
                    TriggerAnimation(ANIMATION.ATTACK_SPECIAL);
                    m_currentDamageCoroutine = ApplyDamageDelayed(1, 30, target);
                    StartCoroutine(m_currentDamageCoroutine);
                }

                m_lastAttackTime = Time.time;
            }
        }
        else
        {
            if (CurrentTarget == null || distanceToTarget > m_attackRange * 2 || Time.time - m_lastAttackTime > attackDelay + 0.2f)
            {
                CurrentTarget = AI.GetNewTarget(transform.position, Platforms.PlayerPlatform);

                if (CurrentTarget == null)
                {
                    PathingTarget = m_chest;
                    UpdatePathTarget(PathingTarget);
                    CurrentTarget = null;
                }
            }

            if (CurrentTarget != null) GetInRange(CurrentTarget.GetTargetableInterface());
        }
    }

    IEnumerator ApplyDamageDelayed(int dmgMultiplier, int frameDelay, IAttackable target)
    {
        yield return new WaitForSeconds(Time.fixedDeltaTime * frameDelay);

        // dont forget animations and such

        target.Damage(this, dmgMultiplier * m_baseDamage * (1 + (m_level - 1) * LevelMultipliers.DAMAGE));
    }

    IEnumerator Freeze(float duration)
    {
        Time.timeScale = 0f;
        float pauseEndTime = Time.realtimeSinceStartup + duration;

        while (Time.realtimeSinceStartup < pauseEndTime)
        {
            yield return null;
        }

        Time.timeScale = 1;
    }

    public void GetInRange(ITargetable target)
    {
        UpdatePathTarget(target);
    }

    public void AfflictStatus(IAttackable target)
    {
        throw new NotImplementedException();
    }

    public Transform TargetTransform(int unitID)
    {
        // TODO add code to locate optimal target for next seeker.
        return transform;
    }

    public Vector3 Position()
    {
        return transform.position;
    }

    public void OnTargetDied(IAttackable target)
    {
        OnEnterPlatform();

        // XP etc.
    }

    public ITargetable GetTargetableInterface()
    {
        return this;
    }
}
