using System;
using PathFinding;
using UnityEngine;
using Entities;
using System.Collections;

/// <summary>
/// Handles control behaviour for player character.
/// </summary>
public class JKnightControl : PathFindingObject, IAttackable, IAttacker, ITargetable
{
    // Variables exposed in the editor.
    [SerializeField] float m_horizontalMod, m_linearMod, m_jumpForce, m_groundTriggerDistance;

    // References to attached components.
    Animator m_animator;
    Rigidbody m_rb;

    // Public property used to check knight focus point
    public Vector3 FocusPoint
    {
        get { return transform.position; }
        set { FocusPoint = value; }
    }

    public float Health { get; set; }

    public int DeathTime { get; set; }

    public IAttackable CurrentTarget { get; set; }

    [SerializeField] float m_attackRange;
    [SerializeField] float m_attacksPerSecond;
    [SerializeField] float m_baseDamage;
    [SerializeField] float m_level;
    [SerializeField] float m_baseHealth;

    float m_lastAttackTime;
    ITargetable m_chest;
    bool m_isDoingSpecial;
    IEnumerator m_currentDamageCoroutine;
    int m_attackState;

    void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_rb = GetComponent<Rigidbody>();
        LineRender = GetComponent<LineRenderer>();
        m_lastAttackTime = -1;
        Health = (m_baseHealth * (1 + (m_level - 1) * LevelMultipliers.HEALTH));
        m_attackState = 0;
        m_currentDamageCoroutine = null;
        GameManager.OnStartRun += OnStartRun;
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

    void ManualInput()
    {
        // Take the input variables from universal input.
        var linearValue = Input.GetAxis("Vertical");
        var lateralValue = Input.GetAxis("Horizontal");

        // Construct a vector that points in the movement direction, modified by scalar.
        var movementVec = new Vector3(lateralValue * Time.deltaTime * m_horizontalMod, 0, linearValue * Time.deltaTime * m_linearMod);

        // This variable will be constructed for later use of LookAt.
        Vector3 lookPoint;

        // Apply movement appropriately and also update the animator and LookAt target.
        if (movementVec != Vector3.zero)
        {
            transform.position += movementVec;
            lookPoint = transform.position + movementVec.normalized;
            m_animator.SetFloat("MovementBlend", 0);

            // Using LookAt is an easy way to maintain the correct visual orientation of the knight.
            transform.LookAt(lookPoint);
        }
        else
        {
            m_animator.SetFloat("MovementBlend", 1);
        }
    }

    // Interacts with universal input
    private void InputHandler()
    {
        //                      TriggerAnimation(ANIMATION.ATTACK_ULTIMATE);
        //                      

        // If waterfall - Chosen as it allows for easy prioritisation of inputs.
        if (Input.GetButtonDown("AttackBasic"))
        {
            TriggerAnimation(ANIMATION.ATTACK_BASIC);
        }
        else if (Input.GetButtonDown("AttackSpecial"))
        {
            TriggerAnimation(ANIMATION.ATTACK_SPECIAL);
        }
        else if (Input.GetButtonDown("AttackUltimate"))
        {
            if (CurrentTarget == null) return;

        }
        else if (Input.GetButtonDown("AttackKick"))
        {
            TriggerAnimation(ANIMATION.ATTACK_KICK);
        }
        else if (Input.GetButtonDown("AttackShield"))
        {
            TriggerAnimation(ANIMATION.ATTACK_SHIELD);
        }
        else if (Input.GetButtonDown("Parry"))
        {
            TriggerAnimation(ANIMATION.PARRY);
        }
        else if (Input.GetButtonDown("Buff"))
        {
            TriggerAnimation(ANIMATION.BUFF);
        }
        else if (Input.GetButtonDown("Death"))
        {
            TriggerAnimation(ANIMATION.DEATH);
        }
        else if (Input.GetButtonDown("Jump"))
        {
            if (!IsAirborne())
            {
                TriggerAnimation(ANIMATION.JUMP);
                m_rb.AddForce(Vector3.up * m_jumpForce);
            }
        }
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
        var nextTarget = AI.GetNewTarget(transform.position, Platforms.PlayerPlatform);

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
        if (target == null) return;

        var distanceToTarget = Vector3.Distance(transform.position, target.Position());

        if (distanceToTarget <= m_attackRange)
        {
            StopMovement();

            transform.LookAt(target.Position());

            if (Input.GetButtonDown("AttackUltimate") && !m_isDoingSpecial)
            {
                InterruptAnimator();
                m_isDoingSpecial = true;
                StopCoroutine(m_currentDamageCoroutine);
                TriggerAnimation(ANIMATION.ATTACK_ULTIMATE);
                m_currentDamageCoroutine = ApplyDamageDelayed(5, 26, target);
                StartCoroutine(m_currentDamageCoroutine);
                return;
            }

            float delay = 1000f / (1000f * m_attacksPerSecond);

            if (!m_isDoingSpecial && (m_lastAttackTime == -1 || Time.time - m_lastAttackTime > delay))
            {
                InterruptAnimator();

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
    }

    IEnumerator ApplyDamageDelayed(int dmgMultiplier, int frameDelay, IAttackable target)
    {
        yield return new WaitForSeconds(Time.fixedDeltaTime * frameDelay);

        // dont forget animations and such

        if (m_isDoingSpecial)
        {
            m_isDoingSpecial = false;
            m_lastAttackTime = Time.time - 0.5f;
            StartCoroutine(Freeze(0.1f));
            
        }

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
