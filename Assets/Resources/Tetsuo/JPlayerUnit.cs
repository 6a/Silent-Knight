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
    public enum ATTACKS { SWORD_SPIN, KICK, SHIELD, PARRY, ULTIMATE }
    private float[] m_currentSkillCD = new float[5];
    private float[] m_baseSkillCD = { 10, 20, 5, 20, 60 };

    // References to attached components.
    Animator m_animator;
    Rigidbody m_rb;
    PlayerWeapon m_weapon;

    // Reference to ui objects
    PlayerHealthBar m_healthbar;
    EnemyHealthBar m_enemyHealthbar;

    // Public property used to check knight focus point
    public Vector3 FocusPoint
    {
        get { return transform.position; }
        set { FocusPoint = value; }
    }

    public Transform GetReferenceTarget()
    {
        return m_refTarget;
    }

    public Transform GetLookTarget()
    {
        return m_lookTarget;
    }

    public Transform GetDeathAnchor()
    {
        return m_deathCamAnchor;
    }

    public float Health { get; set; }

    public int DeathTime { get; set; }

    public IAttackable CurrentTarget { get; set; }

    public int ID { get; set; }

    public bool FoundBoss { get; set; }

    public bool IsBoss { get; set; }

    [SerializeField] float m_attackRange;
    [SerializeField] float m_attacksPerSecond;
    [SerializeField] float m_baseDamage;
    [SerializeField] int m_level;
    [SerializeField] int m_xp;
    [SerializeField] float m_baseHealth;
    [SerializeField] float m_buffMultiplier;
    [SerializeField] float m_critChance;
    [SerializeField] float m_critMultiplier;
    [SerializeField] float m_damageboost;
    [SerializeField] float m_dodgeChance;
    [SerializeField] float m_regenAmount;
    [SerializeField] float m_regenDelay;
    [SerializeField] float m_regenTick;
    [SerializeField] float m_ultDuration;
    [SerializeField] float m_buffRotInterval;
    [SerializeField] float m_buffRotPercentDamage;
    [SerializeField] float m_dangerHealthThreshold;
    [SerializeField] GameObject m_psBuff, m_psBuffStart, m_psDeflect, m_psSwordClash, m_psKickConnection;
    [SerializeField] GameObject m_toe, m_swordContact;
    [SerializeField] Transform m_refTarget, m_lookTarget;
    [SerializeField] Transform m_deathCamAnchor;
    [SerializeField] ParticleSystem m_psSwordSlash;

    ITargetable m_endtarget;

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
    bool m_isStartingSpec;

    int m_attackState;

    void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_rb = GetComponent<Rigidbody>();
        LineRenderer = GetComponent<LineRenderer>();
        m_weapon = GetComponentInChildren<PlayerWeapon>();
        m_lastAttackTime = -1;
        m_xp = PPM.LoadInt(PPM.KEY_INT.XP);
        m_attackState = 0;
        GameManager.RegisterPlayer(this);

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

            FCTRenderer.AddFCT(FCT_TYPE.HEALTH, "+ " + addedHealth.ToString("F0"), transform.position + Vector3.up, Vector2.down);

            m_healthbar.UpdateHealthDisplay(Health / m_maxHealth, (int)m_maxHealth);
            m_healthbar.Pulse(true);

            m_nextRegenTick += m_regenTick;
        }

        UpdateCooldownSpinners();

        if (Parry()) return;

        if (Buff()) return;

        Attack(CurrentTarget);
    }

    internal void SetAttackRange(float v)
    {
        m_attackRange = v;
    }

    private void FixedUpdate()
    {
        UpdateMovement();
    }

    void UpdateLevel(int experienceGained, bool init = false)
    {
        if (!init) m_xp += experienceGained;
        PPM.SaveInt(PPM.KEY_INT.XP, m_xp);

        var level = LevelScaling.GetLevel(m_xp);

        float currentLevelXP = LevelScaling.GetXP(level + 1) - LevelScaling.GetXP(level);
        var fillAmount = (m_xp - LevelScaling.GetXP(level)) / currentLevelXP;

        if (level != m_level)
        {
            GameUIManager.TriggerLevelUpAnimation();
            GameUIManager.UpdatePlayerLevel(level, fillAmount);

            for (int i = 0; i < m_currentSkillCD.Length; i++)
            {
                m_currentSkillCD[i] = 0;
                GameUIManager.UpdateSpinner((ATTACKS)i, 0, 0);
            }

            m_maxHealth = BonusManager.GetModifiedValue(BONUS.HEALTH_BOOST, LevelScaling.GetScaledHealth(level, (int)m_baseHealth));
            Health = m_maxHealth;
            m_healthbar.UpdateHealthDisplay(1, (int)m_maxHealth);

            if (!init) BonusManager.AddCredits(LevelScaling.CreditsPerLevel, true);
        }
        else
        {
            GameUIManager.UpdatePlayerLevel(level, fillAmount);
        }

        m_level = level;
    }

    public void UpdateHealthDisplay()
    {
        m_maxHealth = BonusManager.GetModifiedValue(BONUS.HEALTH_BOOST, LevelScaling.GetScaledHealth(m_level, (int)m_baseHealth));
        Health = m_maxHealth;
        m_healthbar.UpdateHealthDisplay(1, (int)m_maxHealth);
    }

    void UpdateCooldownSpinners()
    {
        for (int i = 0; i < m_currentSkillCD.Length; i++)
        {
            if (m_currentSkillCD[i] > 0)
            {
                m_currentSkillCD[i] = Mathf.Max(m_currentSkillCD[i] - Time.deltaTime, 0);

                var cd = 0f;

                switch ((ATTACKS)i)
                {
                    case ATTACKS.SWORD_SPIN:
                        cd =  BonusManager.GetModifiedValue(BONUS.CD_SPIN, m_baseSkillCD[i]);
                        break;
                    case ATTACKS.KICK:
                        cd =  BonusManager.GetModifiedValue(BONUS.CD_KICK, m_baseSkillCD[i]);
                        break;
                    case ATTACKS.SHIELD:
                        cd =  BonusManager.GetModifiedValue(BONUS.CD_SHIELD_BASH, m_baseSkillCD[i]);
                        break;
                    case ATTACKS.PARRY:
                        cd =  BonusManager.GetModifiedValue(BONUS.CD_DEFLECT, m_baseSkillCD[i]);
                        break;
                    case ATTACKS.ULTIMATE:
                        cd =  BonusManager.GetModifiedValue(BONUS.CD_ULT, m_baseSkillCD[i]);
                        break;
                }

                var value = m_currentSkillCD[i] / cd;
                var seconds = m_baseSkillCD[i] - (m_baseSkillCD[i] - m_currentSkillCD[i]);

                GameUIManager.UpdateSpinner((ATTACKS)i, value, seconds);
            }
        }
    }

    public override void OnFollowPath(float speedPercent)
    {
        if (speedPercent > 0) speedPercent = Mathf.Clamp01(speedPercent + 0.5f);

        m_animator.SetFloat("MovementBlend", 1 - speedPercent);
    }

    public void EnableWCollider()
    {
        m_weapon.Switch(true);
        StartCooldown(ATTACKS.SWORD_SPIN);
    }

    void DisableWCollider()
    {
        m_weapon.Switch(false);
    }

    public void OnSlashStart()
    {
        m_psSwordSlash.Stop();
        m_psSwordSlash.Play();
    }

    // Triggers appropriate animation. Is set to interrupt the current animation, and then trigger
    // the appropriate one.
    private void TriggerAnimation(ANIMATION anim, bool interrupt = true)
    {
        if (interrupt) InterruptAnimator();
        DisableWCollider();

        switch (anim)
        {
            case ANIMATION.ATTACK_BASIC1:
                m_animator.SetTrigger("A1Start");
                break;
            case ANIMATION.ATTACK_BASIC2:
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

            ApplyDamage(enemy, 3, true);

            Audio.PlayFX(Audio.FX.SWORD_IMPACT, transform.position);

            StartCoroutine(Freeze(0.05f, false, true));

            Instantiate(m_psSwordClash, m_swordContact.transform.position, Quaternion.identity);

            enemy.KnockBack(new Vector2(transform.position.x, transform.position.z), 400);
            print("!");
        }
    }

    public void ApplyDamage(IAttackable enemy, int baseDamageMultiplier, bool spin = false)
    {
        try
        {
            var rand = UnityEngine.Random.Range(0f, 1f);

            var isCrit = rand < BonusManager.GetModifiedValueFlatAsDecimal(BONUS.CRIT_CHANCE, m_critChance);
            var critMultiplier = (isCrit) ? m_critMultiplier : 1;

            var dmg = BonusManager.GetModifiedValue(BONUS.DAMAGE_BOOST, LevelScaling.GetScaledDamage(m_level, (int)m_baseDamage));

            var total = baseDamageMultiplier * GetValue(BONUS.DAMAGE_BOOST) * dmg * critMultiplier;

            var t = (isCrit) ? FCT_TYPE.CRIT : FCT_TYPE.HIT;

            enemy.Damage(this, total, t);
        }
        catch (Exception)
        {
        }
    }

    public void ApplyPercentageDamage(IAttackable enemy, float amount)
    {
        var rand = UnityEngine.Random.Range(0f, 1f);

        var isCrit = rand < BonusManager.GetModifiedValue(BONUS.CRIT_CHANCE, m_critChance);

        var critMultiplier = (isCrit) ? m_critMultiplier : 1;

        if (amount > 0)
        {
            amount *= -1;
        }

        var t = (isCrit) ? FCT_TYPE.DOTCRIT : FCT_TYPE.DOTHIT;

        if (enemy != null) enemy.Damage(this, amount * critMultiplier, t);
    }

    public float GetValue(BONUS bonus)
    {
        switch (bonus)
        {
            case BONUS.CRIT_CHANCE:             return 100 * m_critChance;
            case BONUS.DAMAGE_BOOST:            return m_damageboost;
            case BONUS.ULT_DURATION_INCREASE:   return m_ultDuration;
            case BONUS.CD_KICK:                 return m_baseSkillCD[(int)ATTACKS.KICK];
            case BONUS.CD_SPIN:                 return m_baseSkillCD[(int)ATTACKS.SWORD_SPIN];
            case BONUS.CD_SHIELD_BASH:          return m_baseSkillCD[(int)ATTACKS.SHIELD];
            case BONUS.CD_DEFLECT:              return m_baseSkillCD[(int)ATTACKS.PARRY];
            case BONUS.CD_ULT:                  return m_baseSkillCD[(int)ATTACKS.ULTIMATE];
            case BONUS.HEALTH_BOOST:            return LevelScaling.GetScaledHealth(m_level, (int)m_baseHealth);
            case BONUS.DODGE_CHANCE:            return 100 * m_dodgeChance;
            default: return 0;
        }
    }

    public void OnShieldAttack()
    {
        try
        {
            var originalTarget = CurrentTarget.ID;

            ApplyDamage(CurrentTarget, 2);

            Audio.PlayFX(Audio.FX.SHIELD_SLAM, transform.position);

            if (originalTarget == CurrentTarget.ID) CurrentTarget.AfflictStatus(STATUS.STUN, 2);

            StartCoroutine(Freeze(0.05f, false, true));

            StartCooldown(ATTACKS.SHIELD);

            m_lastAttackTime = Time.time - 0.5f;
            OnFollowPath(0);
        }
        catch (Exception)
        {
            m_isStartingSpec = false;
            m_lastAttackTime = Time.time - 0.5f;
        }
    }

    public void OnKick()
    {
        try
        {
            var originalTarget = CurrentTarget.ID;

            ApplyDamage(CurrentTarget, 1);

            Audio.PlayFX(Audio.FX.KICK, transform.position);

            StartCoroutine(Freeze(0.05f, false, true));

            Instantiate(m_psKickConnection, m_toe.transform.position, Quaternion.identity);

            if (originalTarget == CurrentTarget.ID) CurrentTarget.KnockBack(new Vector2(transform.position.x, transform.position.z), 800);
            StartCooldown(ATTACKS.KICK);
            m_lastAttackTime = Time.time - 0.5f;
            OnFollowPath(0);

        }
        catch (Exception)
        {
            m_isStartingSpec = false;
            m_lastAttackTime = Time.time - 0.5f;
        }
    }

    public void SwordSpinFinished()
    {
        DisableWCollider();
        m_lastAttackTime = Time.time - 0.5f;
        enemiesHit = new List<int>();
        OnFollowPath(0);
    }

    public void OnParryFinished()
    {
        if (m_parrySuccess)
        {
            StartCooldown(ATTACKS.PARRY, 0.125f, 0.5f);
        }
        else
        {
            StartCooldown(ATTACKS.PARRY);
        }

        m_isParrying = false;
        OnFollowPath(0);
    }

    void StartCooldown(ATTACKS attack, float multiplier = 1, float reduceAttackDelay = 0)
    {
        float cd = 0;

        switch (attack)
        {
            case ATTACKS.SWORD_SPIN:
                cd = multiplier * BonusManager.GetModifiedValue(BONUS.CD_SPIN, m_baseSkillCD[(int)attack]);
                break;
            case ATTACKS.KICK:
                cd = multiplier * BonusManager.GetModifiedValue(BONUS.CD_KICK, m_baseSkillCD[(int)attack]);
                break;
            case ATTACKS.SHIELD:
                cd = multiplier * BonusManager.GetModifiedValue(BONUS.CD_SHIELD_BASH, m_baseSkillCD[(int)attack]);
                break;
            case ATTACKS.PARRY:
                cd = multiplier * BonusManager.GetModifiedValue(BONUS.CD_DEFLECT, m_baseSkillCD[(int)attack]);
                break;
            case ATTACKS.ULTIMATE:
                cd = multiplier * BonusManager.GetModifiedValue(BONUS.CD_ULT, m_baseSkillCD[(int)attack]);
                break;
        }

        m_currentSkillCD[(int)attack] = cd;

        m_lastAttackTime = Time.time - reduceAttackDelay;
        m_isStartingSpec = false;
    }

    public bool UltIsOnCooldown()
    {
        return (m_currentSkillCD[(int)ATTACKS.ULTIMATE] > 0);
    }

    public void OnEnterPlatform()
    {
        IAttackable nextTarget;

        nextTarget = AI.GetNewTarget(transform.position, Platforms.PlayerPlatform);

        if (nextTarget == null)
        {
            PathingTarget = m_endtarget;
            UpdatePathTarget(PathingTarget);
            CurrentTarget = null;
        }
        else
        {
            if (CurrentTarget != null) CurrentTarget.SetRenderTarget(false);
            CurrentTarget = nextTarget;
            CurrentTarget.SetRenderTarget(true);

            GetInRange(nextTarget.GetTargetableInterface());
        }
    }

    public override void OnStartRun()
    {
        Platforms.RegisterPlayer(this);

        GameUIManager.Show(true);

        PreviousPos = transform.position;

        CurrentTarget = null;

        PathingTarget = FindObjectOfType<Chest>();
        if (PathingTarget == null)
            PathingTarget = GameObject.FindGameObjectWithTag("Boss").GetComponent<JEnemyUnit>() as ITargetable;

        m_endtarget = PathingTarget;

        m_healthbar = FindObjectOfType<PlayerHealthBar>();

        UpdateLevel(m_xp, true);
        m_maxHealth = Health;

        m_healthbar.UpdateHealthDisplay(1, (int)m_maxHealth);

        m_enemyHealthbar = FindObjectOfType<EnemyHealthBar>();

        GameUIManager.SetCurrentPlayerReference(this);

        m_level = LevelScaling.GetLevel(m_xp);

        Running = true;

        UpdatePathTarget(PathingTarget);
        StartCoroutine(RefreshPath());

        UpdateBonusDisplay();
    }

    public void UpdateBonusDisplay()
    {
        for (int i = 0; i < 10; i++)
        {
            BonusManager.UpdateBonusDisplay((BONUS)i, this);
        }
    }

    public override void OnEndRun()
    {

    }

    public void Damage(IAttacker attacker, float dmg, FCT_TYPE type)
    {
        if (IsDead) return;

        if (UnityEngine.Random.Range(0f, 1f) <= BonusManager.GetModifiedValueFlatAsDecimal(BONUS.DODGE_CHANCE, m_dodgeChance))
        {
            FCTRenderer.AddFCT(FCT_TYPE.DODGE, "!", transform.position + (1.1f * Vector3.up), Vector3.up);
            return;
        }

        Health -= Mathf.Max(dmg, 0);

        m_nextRegenTick = Time.time + m_regenDelay;

        FCTRenderer.AddFCT(type, dmg.ToString("F0"), transform.position + Vector3.up);

        m_healthbar.UpdateHealthDisplay(Mathf.Max(Health / m_maxHealth, 0), (int)m_maxHealth);

        if (Health <= 0)
        {
            print("Player died");
            Running = false;
            TriggerAnimation(ANIMATION.DEATH);
            IsDead = true;
            attacker.OnTargetDied(this);
        }
    }

    public void OnDeathAnimationFinished()
    {
        CameraFollow.SwitchViewDeath();
    }

    public void KnockBack(Vector2 sourcePos, float strength)
    {
        throw new NotImplementedException();
    }

    public void AfflictStatus(STATUS status, float duration)
    {
        throw new NotImplementedException();
    }

    void OnTriggerStay(Collider c)
    {
        if (!Running) return;

        if (m_applyBuffDamage && Time.time >= m_nextBuffRotTime)
        {
            if (c.gameObject.layer == 10 || c.gameObject.layer == 12)
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
                Audio.PlayFX(Audio.FX.DEFLECT, transform.position);

                StartCoroutine(Freeze(0.05f, false, true));

                var rand = UnityEngine.Random.Range(0f, 1f);

                var isCrit = rand < BonusManager.GetModifiedValue(BONUS.CRIT_CHANCE, m_critChance);
                var critMultiplier = (isCrit) ? m_critMultiplier : 1;

                projectile.Crit = isCrit;

                var dmg = BonusManager.GetModifiedValue(BONUS.DAMAGE_BOOST, LevelScaling.GetScaledDamage(m_level, (int)m_baseDamage));

                projectile.Reflect(this, 5, -1, dmg * 3 * critMultiplier);
                Instantiate(m_psDeflect, projectile.transform.position, Quaternion.identity);
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
        if (!m_isStartingSpec && (Input.GetButtonDown("Parry") || m_simTriggers[(int)ATTACKS.PARRY]))
        {
            if (m_currentSkillCD[(int)ATTACKS.PARRY] <= 0)
            {
                m_isStartingSpec = true;
                TriggerAnimation(ANIMATION.PARRY);
                m_lastAttackTime = Time.time + 10;
                return true;
            }
            else
            {
                GameUIManager.Pulse(ATTACKS.PARRY);
            }
        }

        return false;
    }

    float m_baseDamageHolder;
    public void OnBuffStart()
    {
        m_baseDamageHolder = m_baseDamage;

        m_baseDamage *= m_buffMultiplier;

        StartCoroutine(BuffTimer(BonusManager.GetModifiedValue(BONUS.ULT_DURATION_INCREASE, m_ultDuration)));

        m_nextBuffRotTime = Time.time;
    }

    float m_speedTemp;

    public void OnBuffAnimationFinished()
    {
        Speed = m_speedTemp;
        m_psBuffStart.SetActive(false);
        StartCooldown(ATTACKS.ULTIMATE, 1, 0.8f);
    }

    public void OnFootStep()
    {
        Audio.PlayFX(Audio.FX.FOOTSTEP, transform.position);
    }

    public bool Buff()
    {
        if (!m_isStartingSpec && (Input.GetButtonDown("Ultimate") || m_simTriggers[(int)ATTACKS.ULTIMATE]))
        {
            if (m_currentSkillCD[(int)ATTACKS.ULTIMATE] <= 0)
            {
                m_isStartingSpec = true;
                TriggerAnimation(ANIMATION.BUFF);
                m_speedTemp = Speed;
                Speed = 0;
                m_psBuffStart.SetActive(true);
                m_lastAttackTime = Time.time + 10;
                return true;
            }
            else
            {
                GameUIManager.Pulse(ATTACKS.ULTIMATE);
            }
        }
        return false;
    }

    IEnumerator BuffTimer(float duration)
    {
        m_applyBuffDamage = true;
        StartCoroutine(Freeze(0.3f, true, true));

        Audio.PlayFX(Audio.FX.BIG_IMPACT);

        yield return new WaitForSeconds(duration - Time.fixedDeltaTime);

        m_psBuff.SetActive(false);

        m_baseDamage = m_baseDamageHolder;

        m_applyBuffDamage = false;

        GameUIManager.UltiState(false);

        Sparky.ResetIntensity(true, 0.5f);
        Audio.BlendMusicTo(Audio.BGM.QUIET, 2);
        GameUIManager.ResetAudioTrigger();
    }

    public void Attack(IAttackable target)
    {
        if (target == null)
        {
            m_enemyHealthbar.ToggleVisibility(false);
        }
        else
        {
            m_enemyHealthbar.ToggleVisibility(true);
            target.SetRenderTarget(true);
        }

        float attackDelay = 1000f / (1000f * m_attacksPerSecond);

        if (!m_isStartingSpec && (Input.GetButtonDown("AttackSpin") || m_simTriggers[(int)ATTACKS.SWORD_SPIN]))
        {
            if (m_currentSkillCD[(int)ATTACKS.SWORD_SPIN] <= 0)
            {
                m_isStartingSpec = true;
                TriggerAnimation(ANIMATION.ATTACK_ULTIMATE);
                m_lastAttackTime = Time.time + 10;
            }
            else
            {
                GameUIManager.Pulse(ATTACKS.SWORD_SPIN);
            }

        }

        if (target != null)
        {
            var distanceToTarget = Vector3.Distance(transform.position, target.Position());

            if (!m_isStartingSpec && (Input.GetButtonDown("AttackKick") || m_simTriggers[(int)ATTACKS.KICK]))
            {
                if (m_currentSkillCD[(int)ATTACKS.KICK] <= 0)
                {
                    if (distanceToTarget < m_attackRange)
                    {
                        m_isStartingSpec = true;
                        TriggerAnimation(ANIMATION.ATTACK_KICK);
                        m_lastAttackTime = Time.time + 10;
                    }
                }
                else
                {
                    GameUIManager.Pulse(ATTACKS.KICK);
                }

            }

            if (!m_isStartingSpec && (Input.GetButtonDown("AttackShield") || m_simTriggers[(int)ATTACKS.SHIELD]))
            {
                if (m_currentSkillCD[(int)ATTACKS.SHIELD] <= 0)
                {
                    if (distanceToTarget < m_attackRange)
                    {
                        m_isStartingSpec = true;
                        TriggerAnimation(ANIMATION.ATTACK_SHIELD);
                        m_lastAttackTime = Time.time + 10;
                    }
                }
                else
                {
                    GameUIManager.Pulse(ATTACKS.SHIELD);
                }

            }

            if (distanceToTarget < m_attackRange)
            {
                if (target.IsBoss)
                {
                    HasReachedBoss = true;
                }

                StopMovement();
                OnFollowPath(0);

                m_nextRegenTick = Time.time + m_regenDelay;

                var targetRotation = Quaternion.LookRotation(target.Position() - transform.position);

                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 4);

                if ((m_lastAttackTime == -1 || Time.time - m_lastAttackTime > attackDelay))
                {
                    m_attackState = (m_attackState == 0) ? 1 : 0;
                    if (m_attackState == 0)
                    {
                        TriggerAnimation(ANIMATION.ATTACK_BASIC1);
                    }
                    else
                    {
                        TriggerAnimation(ANIMATION.ATTACK_BASIC2);
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
                        PathingTarget = m_endtarget;
                        UpdatePathTarget(PathingTarget);
                        CurrentTarget = null;
                        m_enemyHealthbar.ToggleVisibility(false);
                    }
                }

                if (CurrentTarget != null) GetInRange(CurrentTarget.GetTargetableInterface());
            }
        }
        else
        {
            try
            {
                var targ = new Vector2(m_endtarget.TargetTransform(0).position.x, m_endtarget.TargetTransform(0).position.z);

                if (!IsFollowingPath && Vector2.SqrMagnitude(targ - new Vector2(transform.position.x, transform.position.z)) < 0.1f)
                {
                    Running = false;
                    GameManager.TriggerLevelLoad();
                }
            }
            catch (Exception)
            {


            }
        }
    }

    public void GiveXp(int xp)
    {
        UpdateLevel(xp);
    }

    public void OnSwordSwingConnect()
    {
        Audio.PlayFX(Audio.FX.SWORD_IMPACT, transform.position);
        Instantiate(m_psSwordClash, m_swordContact.transform.position, Quaternion.identity);

        ApplyDamage(CurrentTarget, 1);
    }

    IEnumerator Freeze(float duration, bool buff = false, bool vibrate = false)
    {
        if (SettingsManager.Haptic())
        {
            if (buff) Sparky.DisableLight();

            Time.timeScale = 0f;
            float pauseEndTime = Time.realtimeSinceStartup + duration;
            while (PauseManager.Paused() || Time.realtimeSinceStartup < pauseEndTime)
            {
                yield return null;
            }
            Time.timeScale = 1;
            if (vibrate) Vibration.Vibrate(Vibration.GenVibratorPattern(0.2f, 50), -1);

        }

        if (buff)
        {
            Sparky.ResetIntensity();
            Sparky.IncreaseIntensity();
            m_psBuff.SetActive(true);
        }
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

    public void OnTargetDied(IAttackable target, bool boss = false)
    {
        if (boss)
        {
            Running = false;
            OnFollowPath(0);
            StopMovement();
            StartCoroutine(BossDead());
        }
        else
        {
            OnEnterPlatform();
        }
    }

    // TODO boss stuff
    IEnumerator BossDead()
    {
        Audio.BlendMusicTo(Audio.BGM.QUIET, 4);

        yield return new WaitForSecondsRealtime(2);

        GameManager.TriggerEndScreen();
    }

    public ITargetable GetTargetableInterface()
    {
        return this;
    }

    public Vector3 GetWorldPos()
    {
        return transform.position;
    }

    public void SetRenderTarget(bool on)
    {
        throw new NotImplementedException();
    }

    bool[] m_simTriggers = new bool[5];

    public void SimulateInputPress(ATTACKS type)
    {
        m_simTriggers[(int)type] = true;
    }

    public void SimulateInputRelease(ATTACKS type)
    {
        m_simTriggers[(int)type] = false;
    }
}
