using System;
using PathFinding;
using UnityEngine;
using Entities;
using System.Collections;

/// <summary>
/// Handles control behaviour for player character.
/// </summary>
public class JKnightControl : PathFindingObject, IEntity, IAttackable, IAttacker, ITargetable
{
    // Variables exposed in the editor.
    [SerializeField] float m_horizontalMod, m_linearMod, m_jumpForce, m_groundTriggerDistance;

    // References to attached components.
    Animator m_animator;
    Rigidbody m_rb;

    public bool Running { get; set; }

    // Public property used to check knight focus point
    public Vector3 FocusPoint
    {
        get { return transform.position; }
        set { FocusPoint = value; }
    }

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

    void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_rb = GetComponent<Rigidbody>();
        LineRender = GetComponent<LineRenderer>();
        m_lastAttackTime = -1;
        Health = (m_baseHealth * (1 + (m_level - 1) * LevelMultipliers.HEALTH));
        GameManager.OnStartRun += OnStartRun;
    }

    void Update()
    {
        // Check for level end trigger
        // GameManager.TriggerLevelLoad();
        //ManualInput();

        // Take current inputs and handle behaviour
        InputHandler();
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
            TriggerAnimation(ANIMATION.ATTACK_ULTIMATE);
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
    private void TriggerAnimation(ANIMATION anim)
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
            case ANIMATION.ATTACK_KICK:
                InterruptAnimator();
                m_animator.SetTrigger("KickStart");
                break;
            case ANIMATION.ATTACK_SHIELD:
                InterruptAnimator();
                m_animator.SetTrigger("ShieldStart");
                break;
            case ANIMATION.PARRY:
                InterruptAnimator();
                m_animator.SetTrigger("ParryStart");
                break;
            case ANIMATION.BUFF:
                InterruptAnimator();
                m_animator.SetTrigger("BuffStart");
                break;
            case ANIMATION.DEATH:
                InterruptAnimator();
                m_animator.SetTrigger("DeathStart");
                break;
            case ANIMATION.JUMP:
                InterruptAnimator();
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
        //print("Player just entered platform: " + CurrentPlatformIndex);


    }

    public void Reset()
    {
        Running = false;
        IsDead = false;
    }

    public override void OnStartRun()
    {
        Platforms.RegisterPlayer(this);

        PreviousPos = transform.position;

        CurrentTarget = null;

        PathingTarget = FindObjectOfType<Chest>();

        UpdatePathTarget(PathingTarget);
        StartCoroutine(RefreshPath());
    }

    public override void OnEndRun()
    {

    }

    public void Damage(float dmg)
    {
        if (IsDead) return;

        Health -= Mathf.Max(dmg, 0);

        print(dmg + " damage received. " + Health + " health remaining.");

        if (Health <= 0)
        {
            print("Player died");
            Running = false;
            InterruptAnimator();
            TriggerAnimation(ANIMATION.DEATH);
            IsDead = true;
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
        throw new NotImplementedException();
    }

    public void GetInRange(ITargetable target)
    {
        throw new NotImplementedException();
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
}
