using UnityEngine;
using PathFinding;
using System;
using Entities;
using System.Collections;
using Localisation;

public enum ENEMY_TYPE { AXE, SPEAR, DAGGER, BOW }

public class JEnemyUnit : PathFindingObject, ITargetable, IAttackable, IAttacker
{
    // References to attached components.
    Animator m_animator;
    Rigidbody m_rb;

    // ui references
    EnemyHealthBar m_healthbar;
    EnemyTitleTextField m_enemyTextField;

    public float Health { get; set; }

    public int DeathTime { get; set; }

    public IAttackable CurrentTarget { get; set; }

    public int ID { get; set; }

    [SerializeField] float m_attackRange;
    [SerializeField] float m_attacksPerSecond;
    [SerializeField] float m_baseDamage;
    [SerializeField] int m_level;
    [SerializeField] float m_baseHealth;
    [SerializeField] SkinnedMeshRenderer m_material;
    [SerializeField] ENEMY_TYPE m_enemyType;
    [SerializeField] GameObject m_projectile;
    [SerializeField] Transform m_projectileTransform;
    [Tooltip("0 = en, 1 = jp")][SerializeField] string[] m_unitName;

    float m_lastAttackTime;
    bool m_deleted = false;

    float m_statusEndTime;
    STATUS m_currentStatus;

    void Awake ()
    {
        m_animator = GetComponent<Animator>();
        LineRender = GetComponent<LineRenderer>();
        m_rb = GetComponent<Rigidbody>();
        m_lastAttackTime = -1;
        Health = CalcuateUnitHealth();
        m_currentStatus = STATUS.NONE;
        GameManager.OnStartRun += OnStartRun;
    }

    float CalcuateUnitHealth()
    {
        return LevelScaling.GetScaledHealth(m_level, (int)m_baseHealth);
    }

    float m_speedTemp;

	void Update ()
    {
        if (!Running) return;
        switch (m_currentStatus)
        {
            case STATUS.STUN:
                if (Time.time > m_statusEndTime)
                {
                    m_animator.enabled = true;
                    m_material.material.color = Color.white;
                    m_currentStatus = STATUS.NONE;
                    Speed = m_speedTemp;
                }
                else
                {
                    if (m_animator.enabled)
                    {
                        m_animator.enabled = false;
                        m_material.material.color = Color.cyan;
                        m_speedTemp = Speed;
                        Speed = 0;
                    }
                }
                break;
            case STATUS.SLOW:
                break;
            case STATUS.CONFUSE:
                break;
            case STATUS.FLINCH:
                break;
            default:
                break;
        }

        if (!m_deleted)
        {
            bool offgrid = ASGrid.IsOffGrid(transform.position);
            if (offgrid || transform.position.y < 0)
            {
                if (offgrid)
                {
                    transform.position = new Vector3(0, -1000, 0);

                    // TODO particles
                }

                ((JPlayerUnit)CurrentTarget).GiveXp((int)CalcuateUnitHealth());

                m_deleted = true;
                Running = false;
                IsDead = true;

                AI.RemoveUnit(Platforms.PlayerPlatform, this);

                Disposal.Dispose(gameObject);
            }
        }

        Attack(CurrentTarget);

        UpdateMovement();
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

        CurrentTarget = FindObjectOfType<JPlayerUnit>();

        PathingTarget = FindObjectOfType<JPlayerUnit>();

        m_healthbar = FindObjectOfType<EnemyHealthBar>();

        m_enemyTextField = FindObjectOfType<EnemyTitleTextField>();

        StartCoroutine(RefreshPath());

        GetInRange(PathingTarget);
    }

    public void Damage(IAttacker attacker, float dmg, FCT_TYPE type)
    {
        if (IsDead || dmg == 0) return;

        // This means we are applying percentage damage
        if (dmg < 0)
        {
            dmg = CalcuateUnitHealth() * (Mathf.Abs(dmg) / 100f);
        }

        dmg *= ((m_currentStatus == STATUS.STUN) ? 2 : 1);
        Health -= dmg;

        m_healthbar.UpdateHealthDisplay(Health / (int)CalcuateUnitHealth(), (int)CalcuateUnitHealth());

        var screenPos = Camera.main.WorldToScreenPoint(transform.position);
        var dir = screenPos - Camera.main.WorldToScreenPoint(attacker.GetWorldPos());

        FCTRenderer.AddFCT(type, dmg.ToString(), transform.position + Vector3.up);

        if (Health <= 0)
        {
            Running = false;
            InterruptAnimator();
            TriggerAnimation(ANIMATION.DEATH);
            IsDead = true;
            attacker.OnTargetDied(this);
            ((JPlayerUnit)attacker).GiveXp((int)CalcuateUnitHealth());
        }
    }

    public void KnockBack(Vector2 sourcePos, float strength)
    {
        var forceVec = (transform.position - new Vector3(sourcePos.x, transform.position.y, sourcePos.y)).normalized * strength;
        m_rb.AddForce(forceVec);
    }

    public void AfflictStatus(STATUS status, float duration)
    {
        RevertAllStatus();
        m_currentStatus = status;
        m_statusEndTime = Time.time + duration;
    }

    void RevertAllStatus()
    {

    }

    public void Attack(IAttackable target)
    {
        var distanceToTarget = Vector3.Distance(transform.position, target.Position());

        if (distanceToTarget <= m_attackRange)
        {
            StopMovement();
            OnFollowPath(0);

            var targetRotation = Quaternion.LookRotation(target.Position() - transform.position);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 4);

            float delay = 1000f / (1000f * m_attacksPerSecond);

            if (m_lastAttackTime == -1 || Time.time - m_lastAttackTime > delay)
            {
                InterruptAnimator();

                PerformAttack(target);
            }
        }
        else
        {
            if (m_currentStatus != STATUS.STUN) GetInRange(CurrentTarget.GetTargetableInterface());
        }
    }

    void PerformAttack(IAttackable target)
    {
        int rand = UnityEngine.Random.Range(0, 30);

        switch (m_enemyType)
        {
            case ENEMY_TYPE.AXE:
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
                break;
            case ENEMY_TYPE.SPEAR:
                if (rand > 2)
                {
                    TriggerAnimation(ANIMATION.ATTACK_BASIC);
                    StartCoroutine(ApplyDamageDelayed(2, 9, target));

                }
                else
                {
                    TriggerAnimation(ANIMATION.ATTACK_ULTIMATE);
                    StartCoroutine(ApplyDamageDelayed(5, 16, target));
                }

                m_lastAttackTime = Time.time;
                break;
            case ENEMY_TYPE.DAGGER:
                if (rand > 2)
                {
                    TriggerAnimation(ANIMATION.ATTACK_BASIC);
                    StartCoroutine(ApplyDamageDelayed(1, 7, target));
                }
                else
                {
                    TriggerAnimation(ANIMATION.ATTACK_SPECIAL);
                    StartCoroutine(ApplyDamageDelayed(2, 7, target));
                    StartCoroutine(ApplyDamageDelayed(2, 19, target));
                }

                m_lastAttackTime = Time.time;
                break;
            case ENEMY_TYPE.BOW:
                TriggerAnimation(ANIMATION.ATTACK_BASIC);

                m_lastAttackTime = Time.time;
                break;
        }
    }

    public void OnProjectileFired()
    {
        var newProjectile = GameObject.Instantiate(m_projectile, m_projectileTransform.position, Quaternion.identity) as GameObject;

        var refToScript = newProjectile.GetComponent<Projectile>();

        refToScript.Init(CurrentTarget, this, 2, 5, m_baseDamage);
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

        target.Damage(this, LevelScaling.GetScaledDamage(m_level, (int)m_baseDamage), FCT_TYPE.HIT);
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

                m_deleted = true;

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

    public Vector3 GetWorldPos()
    {
        return transform.position;
    }

    public void SetRenderTarget(bool on)
    {
        GetComponentInChildren<Camera>().enabled = on;

        if (on)
        {
            gameObject.SetLayerRecursively(12);
            m_healthbar.UpdateHealthDisplay(Health / CalcuateUnitHealth(), (int)CalcuateUnitHealth());
            m_enemyTextField.UpdateTextData(m_unitName[0], m_unitName[1]);
        }
        else
        {
            gameObject.SetLayerRecursively(10);
        }
    }

    public void SetLevel(int level)
    {
        m_level = level;
    }
}
