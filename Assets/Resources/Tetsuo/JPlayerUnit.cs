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
    public enum SPECIAL { ULTIMATE, KICK, SHIELD, PARRY, BUFF }

    // Variables exposed in the editor.
    [SerializeField] float m_horizontalMod, m_linearMod, m_jumpForce, m_groundTriggerDistance;

    // References to attached components.
    Animator m_animator;
    Rigidbody m_rb;
    PlayerWeapon m_weapon;

    // Reference to healthbar object
    HealthBar m_hb;

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
    [SerializeField] float m_buffMultiplier;
    [SerializeField] float m_critChance;
    [SerializeField] float m_critMultiplier;
    [SerializeField] float m_regenAmount;
    [SerializeField] float m_regenDelay;
    [SerializeField] float m_regenTick;
    [SerializeField] float m_shieldAttackCooldown;
    [SerializeField] float m_ultCooldown;
    [SerializeField] float m_parryCooldown;
    [SerializeField] float m_kickCooldown;
    [SerializeField] float m_buffCooldown;
    [SerializeField] float m_buffDuration;
    [SerializeField] float m_buffRotInterval;
    [SerializeField] float m_buffRotPercentDamage;
    [SerializeField] float m_dangerHealthThreshold;
    [SerializeField] GameObject m_buffSystem, m_buffInitSystem, m_hpAnchor;

    ITargetable m_chest;

    bool m_isDoingSpecial;
    bool m_parrySuccess;
    bool m_applyBuffDamage;

    float m_lastAttackTime;
    float m_nextUltTime;
    float m_nextKickTime;
    float m_nextShieldAttackTime;
    float m_nextParryTime;
    float m_nextBuffTime;
    float m_buffEndTime;
    float m_nextBuffRotTime;
    float m_maxHealth;
    float m_nextRegenTick;

    bool m_isParrying;
    bool m_tickRot;

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
        m_maxHealth = Health;
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

        if (m_tickRot)
        {
            m_nextBuffRotTime = Time.time + m_buffRotInterval;
            m_tickRot = false;
        }

        if (Time.time > m_nextRegenTick && Health < m_maxHealth)
        {
            var addedHealth = (m_maxHealth * m_regenAmount);
            Health = Mathf.Clamp(Health + addedHealth, 0, m_maxHealth);

            FCTRenderer.AddFCT(FCT_TYPE.HEALTH, "+ " + addedHealth.ToString(), transform.position + Vector3.up, Vector2.down);

            m_hb.UpdateHealthDisplay(Health / m_maxHealth);
            m_hb.Pulse(true);

            m_nextRegenTick += m_regenTick;
        }

        if (Parry()) return;

        if (Buff()) return;

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

        if (!enemiesHit.Contains(enemy.ID))
        {
            enemiesHit.Add(enemy.ID);

            ApplyDamage(enemy, 5);

            StartCoroutine(Freeze(0.1f));

            // TODO particles

            enemy.KnockBack(new Vector2(transform.position.x, transform.position.z), 400);
        }
    }

    public void ApplyDamage(IAttackable enemy, int baseDamageMultiplier)
    {
        var rand = UnityEngine.Random.Range(0f, 1f);

        var isCrit = rand < m_critChance;

        var critMultiplier = (isCrit) ? m_critMultiplier : 1;

        var total = m_baseDamage * (1 + (m_level - 1) * LevelMultipliers.DAMAGE) * baseDamageMultiplier * critMultiplier;

        var t = (isCrit) ? FCT_TYPE.CRIT : FCT_TYPE.HIT;

        enemy.Damage(this, total, t);
    }

    public void ApplyPercentageDamage(IAttackable enemy, float amount)
    {
        var rand = UnityEngine.Random.Range(0f, 1f);

        var isCrit = rand < m_critChance;

        var critMultiplier = (isCrit) ? m_critMultiplier : 1;

        if (amount > 0)
        {
            amount *= -1;
        }

        var t = (isCrit) ? FCT_TYPE.DOTCRIT : FCT_TYPE.DOTHIT;

        enemy.Damage(this, amount * critMultiplier, t);
    }

    public void OnShieldAttack()
    {
        if (CurrentTarget == null) return;

        ApplyDamage(CurrentTarget, 2);

        StartCoroutine(Freeze(0.1f));

        // TODO particles

        CurrentTarget.AfflictStatus(STATUS.STUN, 2);


        ToggleSpecial(SPECIAL.SHIELD, false, m_shieldAttackCooldown);
        m_lastAttackTime = Time.time - 0.5f;
        OnFollowPath(0);
    }

    public void OnKick()
    {
        if (CurrentTarget == null) return;

        ApplyDamage(CurrentTarget, 1);

        StartCoroutine(Freeze(0.1f));

        // TODO particles

        CurrentTarget.KnockBack(new Vector2(transform.position.x, transform.position.z), 800);

        ToggleSpecial(SPECIAL.KICK, false, m_kickCooldown);
        m_lastAttackTime = Time.time - 0.5f;
        OnFollowPath(0);
    }

    public void UltimateFinished()
    {
        DisableWCollider();
        ToggleSpecial(SPECIAL.ULTIMATE, false);
        m_lastAttackTime = Time.time - 0.5f;
        enemiesHit = new List<int>();
        OnFollowPath(0);
    }

    public void OnParryFinished()
    {
        ToggleSpecial(SPECIAL.PARRY, false);

        if (m_parrySuccess)
        {
            m_lastAttackTime = Time.time - 0.5f;
        }
        else
        {
            m_lastAttackTime = Time.time + 0.5f;
            StartCoroutine(LockSpecial(1));
        }

        m_isParrying = false;
        OnFollowPath(0);
    }

    IEnumerator LockSpecial(float duration)
    {
        m_isDoingSpecial = true;
        yield return new WaitForSeconds(duration);

        m_isDoingSpecial = false;
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

        m_hb = FindObjectOfType<HealthBar>();
        m_hb.Init(m_hpAnchor.transform);

        UpdatePathTarget(PathingTarget);
        StartCoroutine(RefreshPath());
    }

    public override void OnEndRun()
    {

    }

    public void Damage(IAttacker attacker, float dmg, FCT_TYPE type)
    {
        if (IsDead) return;

        Health -= Mathf.Max(dmg, 0);

        m_hb.UpdateHealthDisplay(Health / m_maxHealth);
        m_nextRegenTick = Time.time + m_regenDelay;

        if (Health / m_maxHealth < m_dangerHealthThreshold)
        {
            m_hb.Pulse(false);
        }

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

    void ToggleSpecial(SPECIAL type, bool on, float cooldown = -1)
    {
        m_isDoingSpecial = on;

        if (cooldown != -1)
        {
            switch (type)
            {
                case SPECIAL.ULTIMATE:
                    m_nextUltTime = Time.time + cooldown;
                    break;
                case SPECIAL.KICK:
                    m_nextKickTime = Time.time + cooldown;
                    break;
                case SPECIAL.SHIELD:
                    m_nextShieldAttackTime = Time.time + cooldown;
                    break;
                case SPECIAL.PARRY:
                    m_nextParryTime = Time.time + cooldown;
                    break;
                case SPECIAL.BUFF:
                    m_nextBuffTime = Time.time + cooldown;
                    break;
            }
        }
    }

    bool CanSpec(SPECIAL type)
    {
        switch (type)
        {
            case SPECIAL.ULTIMATE:
                return Time.time > m_nextUltTime;
            case SPECIAL.KICK:
                return Time.time > m_nextKickTime;
            case SPECIAL.SHIELD:
                return Time.time > m_nextShieldAttackTime;
            case SPECIAL.PARRY:
                return Time.time > m_nextParryTime;
            case SPECIAL.BUFF:
                return Time.time > m_nextBuffTime;
        }

        return true;
    }

    public int GetCooldownPercent(SPECIAL type)
    {
        int percent = -1;

        switch (type)
        {
            case SPECIAL.ULTIMATE:
                percent = (int)(((m_nextUltTime - Time.time) / m_ultCooldown) * 100);
                break;
            case SPECIAL.KICK:
                percent = (int)(((m_nextKickTime - Time.time) / m_kickCooldown) * 100);
                break;
            case SPECIAL.SHIELD:
                percent = (int)(((m_nextShieldAttackTime - Time.time) / m_shieldAttackCooldown) * 100);
                break;
            case SPECIAL.PARRY:
                percent = (int)(((m_nextParryTime - Time.time) / m_parryCooldown) * 100);
                break;
            case SPECIAL.BUFF:
                percent = (int)(((m_nextBuffTime - Time.time) / m_buffCooldown) * 100);
                break;
        }

        if (percent != -1)
        {
            percent = 100 - Mathf.Clamp(percent, 0, 100);
        }

        return percent;
    }

    void OnTriggerStay(Collider c)
    {
        if (m_applyBuffDamage && Time.time >= m_nextBuffRotTime)
        {
            if (c.gameObject.layer == 10)
            {
                m_tickRot = true;   

                var enemy = c.GetComponent<JEnemyUnit>() as IAttackable;

                ApplyPercentageDamage(enemy, -m_buffRotPercentDamage);
            }
        }

        if (!m_isParrying) return;

        if (c.gameObject.layer == 9)
        {
            var projectile = c.GetComponent<Projectile>();

            if (projectile.CanBeReflected(this))
            {
                StartCoroutine(Freeze(0.1f));

                var rand = UnityEngine.Random.Range(0f, 1f);

                var isCrit = rand < m_critChance;

                var critMultiplier = (isCrit) ? m_critMultiplier : 1;

                projectile.Crit = isCrit;
                projectile.Reflect(this, 5, -1, m_baseDamage * 3 * critMultiplier);
                m_parrySuccess = true;
            }
        }
    }

    public void OnParryStart()
    {
        m_isParrying = true;
        m_parrySuccess = false;
    }

    public bool Parry()
    {
        if (!m_isDoingSpecial)
        {
            if (CanSpec(SPECIAL.ULTIMATE) && Input.GetButtonDown("Parry"))
            {
                ToggleSpecial(SPECIAL.PARRY, true, m_parryCooldown);
                if (m_currentDamageCoroutine != null) StopCoroutine(m_currentDamageCoroutine);
                TriggerAnimation(ANIMATION.PARRY);
                return true;
            }
        }

        return false;
    }

    float m_baseDamageHolder;

    public void OnBuffStart()
    {
        m_buffEndTime = Time.time + m_buffDuration;

        m_baseDamageHolder = m_baseDamage;

        m_baseDamage *= m_buffMultiplier;

        StartCoroutine(Freeze(0.3f));

        StartCoroutine(BuffTimer(m_buffDuration));

        // particles and stuff

        m_nextBuffRotTime = Time.time;
    }

    float m_speedTemp;

    public void OnBuffAnimationFinished()
    {
        Speed = m_speedTemp;
        ToggleSpecial(SPECIAL.BUFF, false);
        m_buffInitSystem.SetActive(false);
    }

    public bool Buff()
    {
        if (!m_isDoingSpecial)
        {
            if (CanSpec(SPECIAL.BUFF) && Input.GetButtonDown("Buff"))
            {
                ToggleSpecial(SPECIAL.BUFF, true, m_buffCooldown);
                if (m_currentDamageCoroutine != null) StopCoroutine(m_currentDamageCoroutine);
                TriggerAnimation(ANIMATION.BUFF);
                m_speedTemp = Speed;
                Speed = 0;
                m_buffInitSystem.SetActive(true);
                return true;
            }
        }

        return false;
    }

    IEnumerator BuffTimer(float duration)
    {
        m_applyBuffDamage = true;

        yield return new WaitForFixedUpdate();

        m_buffSystem.SetActive(true);

        yield return new WaitForSeconds(duration - Time.fixedDeltaTime);

        // Turn off animations and stuff

        m_buffSystem.SetActive(false);

        m_baseDamage = m_baseDamageHolder;

        m_applyBuffDamage = false;
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

            m_nextRegenTick = Time.time + m_regenDelay;

            var targetRotation = Quaternion.LookRotation(target.Position() - transform.position);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 4);

            if (!m_isDoingSpecial)
            {
                if (CanSpec(SPECIAL.ULTIMATE) && Input.GetButtonDown("AttackUltimate"))
                {
                    ToggleSpecial(SPECIAL.ULTIMATE, true, m_ultCooldown);
                    if (m_currentDamageCoroutine != null) StopCoroutine(m_currentDamageCoroutine);
                    TriggerAnimation(ANIMATION.ATTACK_ULTIMATE);
                    return;
                }

                if (CanSpec(SPECIAL.KICK) && Input.GetButtonDown("AttackKick"))
                {
                    ToggleSpecial(SPECIAL.KICK, true, m_kickCooldown);
                    if (m_currentDamageCoroutine != null) StopCoroutine(m_currentDamageCoroutine);
                    TriggerAnimation(ANIMATION.ATTACK_KICK);
                    return;
                }

                if (CanSpec(SPECIAL.SHIELD) && Input.GetButtonDown("AttackShield"))
                {
                    ToggleSpecial(SPECIAL.SHIELD, true, m_shieldAttackCooldown);
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

        ApplyDamage(target, dmgMultiplier);
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

    public Vector3 GetWorldPos()
    {
        return transform.position;
    }
}
