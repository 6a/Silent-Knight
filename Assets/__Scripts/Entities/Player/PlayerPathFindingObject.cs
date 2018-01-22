using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SilentKnight.AI;
using SilentKnight.Audio;
using SilentKnight.CameraTools;
using SilentKnight.DungeonGeneration;
using SilentKnight.Utility;
using SilentKnight.Engine;
using SilentKnight.PathFinding;
using SilentKnight.FCT;
using SilentKnight.UI.Bonus;
using SilentKnight.UI.Game;
using SilentKnight.UI.Pause;
using SilentKnight.UI.Settings;

namespace SilentKnight.Entities
{
    /// <summary>
    /// Handles control behaviour for player character.
    /// </summary>
    public class PlayerPathFindingObject : PathFindingObject, IAttackable, IAttacker, ITargetable
    {
        #region 00_MEMBER_VARIABLES_AND_PROPERTIES --------------------------------------------------------------------------------------------
        // Const object data
        readonly PlayerCooldownConstants BASE_COOLDOWN = new PlayerCooldownConstants(5, 10, 5, 10, 60);

        // State variables
        PlayerCooldowns m_currentCooldowns = new PlayerCooldowns();
        PlayerStateData m_playerStateData = new PlayerStateData();
        SimulatedInput m_simulatedInput = new SimulatedInput();

        // References to attached components.
        Animator m_animator;
        Rigidbody m_rb;
        PlayerWeapon m_weapon;

        // Reference to UI objects
        EnemyHealthBar m_enemyHealthbar;
        PlayerHealthBar m_healthbar;

        // Properties
        public IAttackable CurrentTarget { get; set; }
        public Vector3 FocusPoint { get { return transform.position; } }
        public float Health { get; set; }
        public int ID { get; set; }
        public bool IsAtBoss { get; set; }
        public bool IsBossUnit { get; set; }

        // Exposed variables for setting unit data
        [SerializeField] float m_attackRange;
        [SerializeField] float m_attacksPerSecond;
        [SerializeField] float m_baseDamage;
        [SerializeField] float m_baseHealth;
        [SerializeField] float m_critChance;
        [SerializeField] float m_critMultiplier;
        [SerializeField] float m_damageboost;
        [SerializeField] float m_dodgeChance;
        [SerializeField] float m_regenAmount;
        [SerializeField] float m_regenDelay;
        [SerializeField] float m_regenTick;
        [SerializeField] float m_ultDuration;
        [SerializeField] float m_ultMultiplier;
        [SerializeField] float m_ultRotPercentDamage;
        [SerializeField] float m_ultRotTickDuration;

        // Exposed variables for setting component/child/instantiation object references
        [SerializeField] Transform m_deathCamAnchor;
        [SerializeField] GameObject m_kickContact;
        [SerializeField] Transform m_lookTarget;
        [SerializeField] GameObject m_particleBuff;
        [SerializeField] GameObject m_particleBuffStart;
        [SerializeField] GameObject m_particleReflect;
        [SerializeField] GameObject m_particleHealStart;
        [SerializeField] GameObject m_particleKick;
        [SerializeField] GameObject m_particleLevelUp;
        [SerializeField] GameObject m_particleSwordClash;
        [SerializeField] ParticleSystem m_particleSwordSpin;
        [SerializeField] Transform m_refTarget;
        [SerializeField] GameObject m_swordContact;
        #endregion 00_MEMBER_VARIABLES_AND_PROPERTIES ---------------------------------------------------------------------------------------------

        #region 01_UNITY_OVERRIDES ------------------------------------------------------------------------------------------------------------
        void Awake()
        {
            LineRenderer = GetComponent<LineRenderer>();
            m_animator = GetComponent<Animator>();
            m_rb = GetComponent<Rigidbody>();
            m_weapon = GetComponentInChildren<PlayerWeapon>();

            m_playerStateData.PreviousAttackTime = -1;
            m_playerStateData.AttackState = 0;
            m_playerStateData.EnemiesHitByWeapon = new List<int>();

            m_playerStateData.XP = PersistentData.LoadInt(PersistentData.KEY_INT.XP);

            // For testing purposes only - This will only evaluate to true on the testing scene.
            if (FindObjectOfType<DungeonTest>() == null)
            {
                GameManager.RegisterPlayer(this);
                GameManager.OnStartRun += OnStartLevel;
            }
        }

        void Start()
        {
            DisableWeaponCollider();
        }

        void Update()
        {
            // Early escape for death, pre-load etc.
            if (!Running) return;

            // Handle ultimate rot ticks
            if (m_playerStateData.ShouldTickUlt)
            {
                m_playerStateData.NextUltTick = Time.time + m_ultRotTickDuration;
                m_playerStateData.ShouldTickUlt = false;
            }

            // Handle out-of-combat heal ticks
            if (Time.time > m_playerStateData.NextRegenTick && Health < m_playerStateData.MaxHealth)
            {
                if (m_playerStateData.IsFirstHealTick)
                {
                    m_particleHealStart.SetActive(false);
                    m_particleHealStart.SetActive(true);
                    m_playerStateData.IsFirstHealTick = false;
                }

                var addedHealth = (m_playerStateData.MaxHealth * m_regenAmount);
                Health = Mathf.Clamp(Health + addedHealth, 0, m_playerStateData.MaxHealth);

                FCTRenderer.AddFCT(Enums.FCT_TYPE.HEALTH, "+ " + addedHealth.ToString("F0"), transform.position + Vector3.up, Vector2.down);

                m_healthbar.UpdateHealthDisplay(Health / m_playerStateData.MaxHealth, (int)m_playerStateData.MaxHealth);

                m_playerStateData.NextRegenTick += m_regenTick;
            }
            else


                // Update UI Cooldown display
                UpdateCooldownSpinners();

            // Handle Reflect activation behaviour
            if (StartReflect()) return;

            // Handle Ultimate activation behaviour
            if (StartUltimate()) return;

            // If this point is reached, the unit is clear to perform an attack (normal, spin or kick)
            PerformAttack(CurrentTarget);
        }

        void FixedUpdate()
        {
            UpdateMovement();
        }

        void OnTriggerStay(Collider c)
        {
            // Early escape for states where no checks should be performed
            if (!Running) return;

            // If an enemy is found within range and the unit is due to apply an ult-rot tick,
            // apply the appropriate damage to the enemy unit and reset the ticker
            // The tick process is run independently within the Update() loop to avoid errors that
            // arise from multiple enemies triggering the collider on the same frame
            if (m_playerStateData.ShouldApplyUltiDamage && Time.time >= m_playerStateData.NextUltTick)
            {
                if (c.gameObject.layer == 10 || c.gameObject.layer == 12)
                {
                    m_playerStateData.ShouldTickUlt = true;

                    var enemy = c.GetComponent<EnemyPathFindingObject>() as IAttackable;

                    ApplyPercentageDamage(enemy, -m_ultRotPercentDamage);
                }
            }

            // If the trigger object is determined to be an enemy projectile and the player
            // unit is currently attempting to reflect it, perform the appropriate actions and
            // reflect the projectile.
            if (m_playerStateData.IsReflecting && c.gameObject.layer == 9)
            {
                var projectile = c.GetComponent<Projectile>();

                if (projectile.CanBeReflected(this))
                {
                    AudioManager.PlayFX(Enums.SFX_TYPE.SPELL_REFLECT, transform.position);

                    StartCoroutine(ApplyHapticFeedback(0.05f, false, true));

                    var rand = UnityEngine.Random.Range(0f, 1f);

                    var isCrit = rand < BonusManager.GetModifiedValue(Enums.PLAYER_STAT.CHANCE_CRIT, m_critChance);
                    var critMultiplier = (isCrit) ? m_critMultiplier : 1;

                    projectile.Crit = isCrit;

                    var dmg = BonusManager.GetModifiedValue(Enums.PLAYER_STAT.BOOST_DAMAGE, LevelScaling.GetScaledDamage(m_playerStateData.Level, (int)m_baseDamage));

                    projectile.Reflect(this, 5, -1, dmg * 3 * critMultiplier);
                    Instantiate(m_particleReflect, projectile.transform.position, Quaternion.identity);
                    m_playerStateData.DidReflect = true;
                }
            }
        }
        #endregion 01_UNITY_OVERRIDES ---------------------------------------------------------------------------------------------------------

        #region 02_PUBLIC_MEMBER_FUNCTIONS ----------------------------------------------------------------------------------------------------
        #region 02A_ANIMATION_TRIGGERS --------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Animation trigger: @frame at which the death animation is considered to be finished.
        /// </summary>
        public void AnimationTriggerDeathFinish()
        {
            CameraFollow.SwitchViewDeath();
        }

        /// <summary>
        /// Animation trigger: @frame at which the reflect animation is considered to be finished.
        /// </summary>
        public void AnimationTriggerReflectFinish()
        {
            if (m_playerStateData.DidReflect)
            {
                StartAttackCooldown(Enums.PLAYER_ATTACK.REFLECT, 0.125f, 0.5f);
            }
            else
            {
                StartAttackCooldown(Enums.PLAYER_ATTACK.REFLECT);
            }

            m_playerStateData.IsReflecting = false;
            OnFollowPath(0);
        }

        /// <summary>
        /// Animation trigger: @frame at which the reflect animation is considered to have started.
        /// </summary>
        public void AnimationTriggerReflectStart()
        {
            m_playerStateData.IsReflecting = true;
            m_playerStateData.DidReflect = false;
        }

        /// <summary>
        /// Animation trigger: @frame at which the running animation puts a food down onto the floor.
        /// </summary>
        public void AnimationTriggerFootstep()
        {
            AudioManager.PlayFX(Enums.SFX_TYPE.FOOTSTEP, transform.position);
        }

        /// <summary>
        /// Animation trigger: @frame at which the kick animation is considered to have started.
        /// </summary>
        public void AnimationTriggerKick()
        {
            try
            {
                var originalTarget = CurrentTarget.ID;

                ApplyFlatDamage(CurrentTarget, 1);

                AudioManager.PlayFX(Enums.SFX_TYPE.KICK, transform.position);

                StartCoroutine(ApplyHapticFeedback(0.05f, false, true));

                Instantiate(m_particleKick, m_kickContact.transform.position, Quaternion.identity);

                if (originalTarget == CurrentTarget.ID) CurrentTarget.OnKnockBack(new Vector2(transform.position.x, transform.position.z), 800);
                StartAttackCooldown(Enums.PLAYER_ATTACK.KICK);
                m_playerStateData.PreviousAttackTime = Time.time - 0.5f;
                OnFollowPath(0);

            }
            catch (Exception)
            {
                m_playerStateData.IsStartingSpec = false;
                m_playerStateData.PreviousAttackTime = Time.time - 0.5f;
            }
        }

        /// <summary>
        /// Animation trigger: @frame at which the standard attack animation is considered to have started.
        /// </summary>
        public void AnimationTriggerRegularAttack()
        {
            AudioManager.PlayFX(Enums.SFX_TYPE.SWORD_IMPACT, transform.position);
            Instantiate(m_particleSwordClash, m_swordContact.transform.position, Quaternion.identity);

            ApplyFlatDamage(CurrentTarget, 1);
        }

        /// <summary>
        /// Animation trigger: @frame at which the shield bash animation is considered to have started.
        /// </summary>
        public void AnimationTriggerShieldBash()
        {
            try
            {
                var originalTarget = CurrentTarget.ID;

                ApplyFlatDamage(CurrentTarget, 2);

                AudioManager.PlayFX(Enums.SFX_TYPE.SHIELD_SLAM, transform.position);

                if (originalTarget == CurrentTarget.ID) CurrentTarget.OnAfflict(Enums.AFFLICTION.STUN, 2);

                StartCoroutine(ApplyHapticFeedback(0.05f, false, true));

                StartAttackCooldown(Enums.PLAYER_ATTACK.SHIELD);

                m_playerStateData.PreviousAttackTime = Time.time - 0.5f;
                OnFollowPath(0);
            }
            catch (Exception)
            {
                m_playerStateData.IsStartingSpec = false;
                m_playerStateData.PreviousAttackTime = Time.time - 0.5f;
            }
        }

        /// <summary>
        /// Animation trigger: @frame at which the sword spin attack animation is considered to have started.
        /// </summary>
        public void AnimationTriggerSpinEnableWeapon()
        {
            m_weapon.Switch(true);
        }

        /// <summary>
        /// Animation trigger: @frame at which the sword spin attack animation is considered to have finished.
        /// </summary>
        public void AnimationTriggerSpinFinish()
        {
            StartAttackCooldown(Enums.PLAYER_ATTACK.SWORD_SPIN);
            DisableWeaponCollider();
            m_playerStateData.PreviousAttackTime = Time.time - 0.5f;
            m_playerStateData.EnemiesHitByWeapon = new List<int>();
            OnFollowPath(0);
        }

        /// <summary>
        /// Animation trigger: @frame at which the sword spin particle effect should be triggered.
        /// </summary>
        public void AnimationTriggerSpinParticle()
        {
            m_particleSwordSpin.Stop();
            m_particleSwordSpin.Play();
        }

        /// <summary>
        /// Animation trigger: @frame at which the ultimate animation is considered to have finished.
        /// </summary>
        public void AnimationTriggerUltimateFinish()
        {
            Speed = m_playerStateData.SpeedTemp;
            m_particleBuffStart.SetActive(false);
            StartAttackCooldown(Enums.PLAYER_ATTACK.ULTIMATE, 1, 0.8f);
        }

        /// <summary>
        /// Animation trigger: @frame at which the ultimate animation is considered to have started.
        /// </summary>
        public void AnimationTriggerUltimateStart()
        {
            m_playerStateData.BaseDamageHolder = m_baseDamage;
            m_baseDamage *= m_ultMultiplier;

            StartCoroutine(UltimateSequence(BonusManager.GetModifiedValue(Enums.PLAYER_STAT.DURATION_INCREASE_ULTIMATE, m_ultDuration)));

            m_playerStateData.NextUltTick = Time.time;
        }
        #endregion 02A_ANIMATION_TRIGGERS -----------------------------------------------------------------------------------------------------

        #region 02B_EVENT_HANDLERS ------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Apply an afflication to this unit.
        /// </summary>
        public void OnAfflict(Enums.AFFLICTION status, float duration)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Damage this unit.
        /// </summary>
        public void OnDamage(IAttacker attacker, float dmg, Enums.FCT_TYPE type)
        {
            if (IsDead) return;

            if (UnityEngine.Random.Range(0f, 1f) <= BonusManager.GetModifiedValueFlatAsDecimal(Enums.PLAYER_STAT.CHANCE_DODGE, m_dodgeChance))
            {
                FCTRenderer.AddFCT(Enums.FCT_TYPE.DODGE, "!", transform.position + (1.1f * Vector3.up), Vector3.up);
                return;
            }

            Health -= Mathf.Max(dmg, 0);

            m_playerStateData.IsFirstHealTick = true;
            m_playerStateData.NextRegenTick = Time.time + m_regenDelay;

            FCTRenderer.AddFCT(type, dmg.ToString("F0"), transform.position + Vector3.up);

            m_healthbar.UpdateHealthDisplay(Mathf.Max(Health / m_playerStateData.MaxHealth, 0), (int)m_playerStateData.MaxHealth);

            if (Health <= 0)
            {
                Running = false;
                TriggerAnimation(Enums.ANIMATION.DEATH);
                IsDead = true;
            }
        }

        /// <summary>
        /// Call this function when this unit enters a platform.
        /// </summary>
        public void OnEnterPlatform()
        {
            IAttackable nextTarget;

            nextTarget = AIManager.GetNewTarget(transform.position, Platforms.PlayerPlatform);

            if (nextTarget == null)
            {
                PathingTarget = m_playerStateData.EndTarget;
                UpdatePathTarget(PathingTarget);
                CurrentTarget = null;
            }
            else
            {
                if (CurrentTarget != null) CurrentTarget.OnSetAsRenderTarget(false);
                CurrentTarget = nextTarget;
                CurrentTarget.OnSetAsRenderTarget(true);

                GetInRange(nextTarget.GetTargetableInterface());
            }
        }

        /// <summary>
        /// Not implemented for this unit. Will throw a NotImplementedException.
        /// </summary>
        public void OnKnockBack(Vector2 sourcePos, float strength)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Call this function when this unit dies.
        /// </summary>
        public void OnTargetDied(IAttackable target, bool boss = false)
        {
            if (boss)
            {
                if ((FindObjectOfType<DungeonGenerator>().IsFinalLevel()))
                {
                    Running = false;
                    OnFollowPath(0);
                    StopMovement();
                    StartCoroutine(BossDeadSequence());
                }
                else
                {
                    GameManager.TriggerLevelLoad();
                }
            }
            else
            {
                OnEnterPlatform();
            }
        }

        /// <summary>
        /// Not implemented for this unit. Will throw a NotImplementedException.
        /// </summary>
        public void OnSetAsRenderTarget(bool on)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Called when the players sword collides with an enemy unit, during a sword spin animation.
        /// </summary>
        public void OnSwordCollision(IAttackable enemy)
        {
            if (enemy == null) return;

            if (!m_playerStateData.EnemiesHitByWeapon.Contains(enemy.ID))
            {
                m_playerStateData.EnemiesHitByWeapon.Add(enemy.ID);

                ApplyFlatDamage(enemy, 2);

                AudioManager.PlayFX(Enums.SFX_TYPE.SWORD_IMPACT, transform.position);

                StartCoroutine(ApplyHapticFeedback(0.05f, false, true));

                Instantiate(m_particleSwordClash, m_swordContact.transform.position, Quaternion.identity);

                enemy.OnKnockBack(new Vector2(transform.position.x, transform.position.z), 400);
            }
        }
        #endregion 02B_EVENT_HANDLERS ---------------------------------------------------------------------------------------------------------

        #region 02C_UPDATERS_AND_SETTERS ------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Add xp to this unit's total xp.
        /// </summary>
        public void GiveXp(int xp)
        {
            UpdateXP(xp);
        }

        /// <summary>
        /// Modify the attack range for this unit. Only used during boss fight phase.
        /// </summary>
        public void SetAttackRange(float v)
        {
            m_attackRange = v;
        }

        /// <summary>
        /// WARNING: BAD OOP! - Forces the bonus manager to update all it's displays. Is called from this class as a reference is
        /// required.
        /// </summary>
        public void UpdateBonusDisplay()
        {
            for (int i = 0; i < 10; i++)
            {
                BonusManager.UpdateBonusDisplay((Enums.PLAYER_STAT)i, this);
            }
        }

        /// <summary>
        /// Update the healthbar to reflect any changes to the maximum health.
        /// </summary>
        public void OnMaxHealthUpdate()
        {
            var healthPercentage = Mathf.Clamp01(Health / m_playerStateData.MaxHealth);

            m_playerStateData.MaxHealth = BonusManager.GetModifiedValue(Enums.PLAYER_STAT.BOOST_HEALTH, LevelScaling.GetScaledHealth(m_playerStateData.Level, (int)m_baseHealth));

            Health = m_playerStateData.MaxHealth * healthPercentage;

            m_healthbar.UpdateHealthDisplay(healthPercentage, (int)m_playerStateData.MaxHealth);
        }
        #endregion 02C_UPDATERS_AND_SETTERS ---------------------------------------------------------------------------------------------------

        #region 02D_GETTERS -------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a reference to this units death camera anchor.
        /// </summary>
        public Transform GetDeathAnchor()
        {
            return m_deathCamAnchor;
        }

        /// <summary>
        /// Returns a reference to this units lookat transform target.
        /// </summary>
        public Transform GetLookTarget()
        {
            return m_lookTarget;
        }

        /// <summary>
        /// Returns the appropriate internal value, after applying all appropriate modifications
        /// such as increase due to level, bonus+- etc.
        /// </summary>
        public float GetPlayerProperty(Enums.PLAYER_STAT bonus)
        {
            switch (bonus)
            {
                case Enums.PLAYER_STAT.CHANCE_CRIT: return 100 * m_critChance;
                case Enums.PLAYER_STAT.BOOST_DAMAGE: return m_damageboost;
                case Enums.PLAYER_STAT.DURATION_INCREASE_ULTIMATE: return m_ultDuration;
                case Enums.PLAYER_STAT.COOLDOWN_KICK: return BASE_COOLDOWN.Get(Enums.PLAYER_ATTACK.KICK);
                case Enums.PLAYER_STAT.COOLDOWN_SPIN: return BASE_COOLDOWN.Get(Enums.PLAYER_ATTACK.SWORD_SPIN);
                case Enums.PLAYER_STAT.COOLDOWN_SHIELD_BASH: return BASE_COOLDOWN.Get(Enums.PLAYER_ATTACK.SHIELD);
                case Enums.PLAYER_STAT.COOLDOWN_REFLECT: return BASE_COOLDOWN.Get(Enums.PLAYER_ATTACK.REFLECT);
                case Enums.PLAYER_STAT.COOLDOWN_ULT: return BASE_COOLDOWN.Get(Enums.PLAYER_ATTACK.ULTIMATE);
                case Enums.PLAYER_STAT.BOOST_HEALTH: return LevelScaling.GetScaledHealth(m_playerStateData.Level, (int)m_baseHealth);
                case Enums.PLAYER_STAT.CHANCE_DODGE: return 100 * m_dodgeChance;
                case Enums.PLAYER_STAT.NULL: return -1;
                default: return 0;
            }
        }

        /// <summary>
        /// Returns this unit's current position.
        /// </summary>
        public Vector3 GetPosition()
        {
            return transform.position;
        }

        /// <summary>
        /// Returns a reference to this unit's current target transform.
        /// </summary>
        public Transform GetReferenceTarget()
        {
            return m_refTarget;
        }

        /// <summary>
        /// Returns this unit's ITargetable interface.
        /// </summary>
        public ITargetable GetTargetableInterface()
        {
            return this;
        }

        /// <summary>
        /// Returns a reference to this unit's transform.
        /// </summary>
        public Transform GetTransform()
        {
            return transform;
        }

        /// <summary>
        /// Returns true if this unit's ultimate is currently on cooldown.
        /// </summary>
        public bool UltIsOnCooldown()
        {
            return (m_currentCooldowns.Get(Enums.PLAYER_ATTACK.ULTIMATE) > 0);
        }
        #endregion 02D_GETTERS ----------------------------------------------------------------------------------------------------------------

        #region 02E_INPUT_HANDLING ------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Simulates a key-down event.
        /// </summary>
        public void SimulateInputPress(Enums.PLAYER_ATTACK type)
        {
            m_simulatedInput.Set(type, true);
        }

        /// <summary>
        /// Simulates a key-up event.
        /// </summary>
        public void SimulateInputRelease(Enums.PLAYER_ATTACK type)
        {
            m_simulatedInput.Set(type, false);
        }
        #endregion 02E_INPUT_HANDLING ---------------------------------------------------------------------------------------------------------
        #endregion 02_PUBLIC_MEMBER_FUNCTIONS -------------------------------------------------------------------------------------------------

        #region 02_PRIVATE_MEMBER_FUNCTIONS ---------------------------------------------------------------------------------------------------
        /// <summary>
        /// Apply a flat amount of damage to the target enemy unit.
        /// </summary>
        void ApplyFlatDamage(IAttackable enemy, int baseDamageMultiplier)
        {
            try
            {
                var rand = UnityEngine.Random.Range(0f, 1f);

                var isCrit = rand < BonusManager.GetModifiedValueFlatAsDecimal(Enums.PLAYER_STAT.CHANCE_CRIT, m_critChance);
                var critMultiplier = (isCrit) ? m_critMultiplier : 1;

                var dmg = BonusManager.GetModifiedValue(Enums.PLAYER_STAT.BOOST_DAMAGE, LevelScaling.GetScaledDamage(m_playerStateData.Level, (int)m_baseDamage));

                var total = baseDamageMultiplier * GetPlayerProperty(Enums.PLAYER_STAT.BOOST_DAMAGE) * dmg * critMultiplier;

                var t = (isCrit) ? Enums.FCT_TYPE.CRIT : Enums.FCT_TYPE.HIT;

                enemy.OnDamage(this, total, t);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Apply a percentage of health as damage to the target enemy unit.
        /// </summary>
        void ApplyPercentageDamage(IAttackable enemy, float amount)
        {
            var rand = UnityEngine.Random.Range(0f, 1f);

            var isCrit = rand < BonusManager.GetModifiedValue(Enums.PLAYER_STAT.CHANCE_CRIT, m_critChance);

            var critMultiplier = (isCrit) ? m_critMultiplier : 1;

            if (amount > 0)
            {
                amount *= -1;
            }

            var t = (isCrit) ? Enums.FCT_TYPE.DOTCRIT : Enums.FCT_TYPE.DOTHIT;

            if (enemy != null) enemy.OnDamage(this, amount * critMultiplier, t);
        }

        /// <summary>
        /// Switch off the collider attached to this unit's weapon.
        /// </summary>
        void DisableWeaponCollider()
        {
            m_weapon.Switch(false);
        }

        /// <summary>
        /// Attempt to get in range of the target.
        /// </summary>
        void GetInRange(ITargetable target)
        {
            UpdatePathTarget(target);
        }

        /// <summary>
        /// Interrupts the attached animator component, and resets all the triggers.
        /// </summary>
        void InterruptAnimator()
        {
            m_animator.SetTrigger("Interrupt");

            m_animator.ResetTrigger("A1Start");
            m_animator.ResetTrigger("A2Start");
            m_animator.ResetTrigger("A3Start");
            m_animator.ResetTrigger("KickStart");
            m_animator.ResetTrigger("ShieldStart");
            m_animator.ResetTrigger("ReflectStart");
            m_animator.ResetTrigger("BuffStart");
            m_animator.ResetTrigger("DeathStart");
            m_animator.ResetTrigger("JumpStart");
        }

        /// <summary>
        /// Attempts to perform an attack. If relevant inputs are detected, non-standard attacks are performed.
        /// </summary>
        void PerformAttack(IAttackable target)
        {
            // Set the enemy icon if an enemy is present. Otherwise hide it.
            if (target == null)
            {
                m_enemyHealthbar.ToggleVisibility(false);
            }
            else
            {
                m_enemyHealthbar.ToggleVisibility(true);
                target.OnSetAsRenderTarget(true);
            }

            // How long to wait between attacks.
            float attackDelay = 1000f / (1000f * m_attacksPerSecond);

            // Perform a spin attack if the trigger is detected and it's not on cooldown etc.
            if (!m_playerStateData.IsStartingSpec && (Input.GetButtonDown("AttackSpin") || m_simulatedInput.Get(Enums.PLAYER_ATTACK.SWORD_SPIN)))
            {
                if (m_currentCooldowns.Get(Enums.PLAYER_ATTACK.SWORD_SPIN) <= 0)
                {
                    m_playerStateData.IsStartingSpec = true;
                    TriggerAnimation(Enums.ANIMATION.ATTACK_ULTIMATE);
                    m_playerStateData.PreviousAttackTime = Time.time + 10;
                }
                else
                {
                    GameUIManager.Pulse(Enums.PLAYER_ATTACK.SWORD_SPIN);
                }

            }

            // Perform a kick attack if the trigger is detected and it's not on cooldown etc.
            if (target != null)
            {
                var distanceToTarget = Vector3.Distance(transform.position, target.GetPosition());

                if (!m_playerStateData.IsStartingSpec && (Input.GetButtonDown("AttackKick") || m_simulatedInput.Get(Enums.PLAYER_ATTACK.KICK)))
                {
                    if (m_currentCooldowns.Get(Enums.PLAYER_ATTACK.KICK) <= 0)
                    {
                        if (distanceToTarget < m_attackRange)
                        {
                            m_playerStateData.IsStartingSpec = true;
                            TriggerAnimation(Enums.ANIMATION.ATTACK_KICK);
                            m_playerStateData.PreviousAttackTime = Time.time + 10;
                        }
                    }
                    else
                    {
                        GameUIManager.Pulse(Enums.PLAYER_ATTACK.KICK);
                    }

                }

                // Perform a shield slam attack if the trigger is detected and it's not on cooldown etc.
                if (!m_playerStateData.IsStartingSpec && (Input.GetButtonDown("AttackShield") || m_simulatedInput.Get(Enums.PLAYER_ATTACK.SHIELD)))
                {
                    if (m_currentCooldowns.Get(Enums.PLAYER_ATTACK.SHIELD) <= 0)
                    {
                        if (distanceToTarget < m_attackRange)
                        {
                            m_playerStateData.IsStartingSpec = true;
                            TriggerAnimation(Enums.ANIMATION.ATTACK_SHIELD);
                            m_playerStateData.PreviousAttackTime = Time.time + 10;
                        }
                    }
                    else
                    {
                        GameUIManager.Pulse(Enums.PLAYER_ATTACK.SHIELD);
                    }

                }

                // Perform a standard attack, or try to get in range if the enemy is too far away.
                if (distanceToTarget < m_attackRange)
                {
                    if (target.IsBossUnit)
                    {
                        HasReachedBoss = true;
                    }

                    StopMovement();
                    OnFollowPath(0);

                    m_playerStateData.NextRegenTick = Time.time + m_regenDelay;

                    var targetRotation = Quaternion.LookRotation(target.GetPosition() - transform.position);

                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 4);

                    if ((m_playerStateData.PreviousAttackTime == -1 || Time.time - m_playerStateData.PreviousAttackTime > attackDelay))
                    {
                        m_playerStateData.AttackState = (m_playerStateData.AttackState == 0) ? 1 : 0;
                        if (m_playerStateData.AttackState == 0)
                        {
                            TriggerAnimation(Enums.ANIMATION.ATTACK_BASIC1);
                        }
                        else
                        {
                            TriggerAnimation(Enums.ANIMATION.ATTACK_BASIC2);
                        }

                        m_playerStateData.PreviousAttackTime = Time.time;
                    }
                }
                else
                {
                    if (CurrentTarget == null || distanceToTarget > m_attackRange * 2 || Time.time - m_playerStateData.PreviousAttackTime > attackDelay + 0.2f)
                    {
                        CurrentTarget = AIManager.GetNewTarget(transform.position, Platforms.PlayerPlatform);

                        if (CurrentTarget == null)
                        {
                            PathingTarget = m_playerStateData.EndTarget;
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
                    // If this stage is reached, the unit is at the end of the level, so trigger a new level load.

                    var targ = new Vector2(m_playerStateData.EndTarget.GetTransform().position.x, m_playerStateData.EndTarget.GetTransform().position.z);

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

        /// <summary>
        /// Put one of the attack types on cooldown.
        /// </summary>
        void StartAttackCooldown(Enums.PLAYER_ATTACK attack, float multiplier = 1, float reduceAttackDelay = 0)
        {
            var cd = multiplier * BonusManager.GetModifiedValue(Enums.ConvertAttackToBonus(attack), BASE_COOLDOWN.Get(attack));
            m_currentCooldowns.Set(attack, cd);

            m_playerStateData.PreviousAttackTime = Time.time - reduceAttackDelay;
            m_playerStateData.IsStartingSpec = false;
        }

        /// <summary>
        /// Returns true if a reflect input event was detected, and was validated.
        /// </summary>
        bool StartReflect()
        {
            if (!m_playerStateData.IsStartingSpec && (Input.GetButtonDown("Reflect") || m_simulatedInput.Get(Enums.PLAYER_ATTACK.REFLECT)))
            {
                if (m_currentCooldowns.Get(Enums.PLAYER_ATTACK.REFLECT) <= 0)
                {
                    m_playerStateData.IsStartingSpec = true;
                    TriggerAnimation(Enums.ANIMATION.REFLECT);
                    m_playerStateData.PreviousAttackTime = Time.time + 10;
                    return true;
                }
                else
                {
                    GameUIManager.Pulse(Enums.PLAYER_ATTACK.REFLECT);
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if an ultimate input event was detected, and was validated.
        /// </summary>
        bool StartUltimate()
        {
            if (!m_playerStateData.IsStartingSpec && (Input.GetButtonDown("Ultimate") || m_simulatedInput.Get(Enums.PLAYER_ATTACK.ULTIMATE)))
            {
                if (m_currentCooldowns.Get(Enums.PLAYER_ATTACK.ULTIMATE) <= 0)
                {
                    m_playerStateData.IsStartingSpec = true;
                    TriggerAnimation(Enums.ANIMATION.BUFF);
                    m_playerStateData.SpeedTemp = Speed;
                    Speed = 0;
                    m_particleBuffStart.SetActive(true);
                    m_playerStateData.PreviousAttackTime = Time.time + 10;
                    return true;
                }
                else
                {
                    GameUIManager.Pulse(Enums.PLAYER_ATTACK.ULTIMATE);
                }
            }
            return false;
        }

        /// <summary>
        /// Cancels any running animations and triggers a new one.
        /// </summary>
        void TriggerAnimation(Enums.ANIMATION anim, bool interrupt = true)
        {
            if (interrupt) InterruptAnimator();
            DisableWeaponCollider();

            switch (anim)
            {
                case Enums.ANIMATION.ATTACK_BASIC1:
                    m_animator.SetTrigger("A1Start");
                    break;
                case Enums.ANIMATION.ATTACK_BASIC2:
                    m_animator.SetTrigger("A2Start");
                    break;
                case Enums.ANIMATION.ATTACK_ULTIMATE:
                    m_animator.SetTrigger("A3Start");
                    break;
                case Enums.ANIMATION.ATTACK_KICK:
                    m_animator.SetTrigger("KickStart");
                    break;
                case Enums.ANIMATION.ATTACK_SHIELD:
                    m_animator.SetTrigger("ShieldStart");
                    break;
                case Enums.ANIMATION.REFLECT:
                    m_animator.SetTrigger("ReflectStart");
                    break;
                case Enums.ANIMATION.BUFF:
                    m_animator.SetTrigger("BuffStart");
                    break;
                case Enums.ANIMATION.DEATH:
                    m_animator.SetTrigger("DeathStart");
                    break;
                case Enums.ANIMATION.JUMP:
                    m_animator.SetTrigger("JumpStart");
                    break;
            }
        }

        /// <summary>
        /// BAD OOP! - Updates all of the UI cooldown objects with current cooldowns.
        /// </summary>
        void UpdateCooldownSpinners()
        {
            for (int i = 0; i < m_currentCooldowns.Count; i++)
            {
                if (m_currentCooldowns.Get((Enums.PLAYER_ATTACK)i) > 0)
                {
                    m_currentCooldowns.Set((Enums.PLAYER_ATTACK)i, Mathf.Max(m_currentCooldowns.Get((Enums.PLAYER_ATTACK)i) - Time.deltaTime, 0));

                    var attackType = (Enums.PLAYER_ATTACK)i;

                    var cd = BonusManager.GetModifiedValue(Enums.ConvertAttackToBonus(attackType), BASE_COOLDOWN.Get(attackType));

                    var fillAmt = m_currentCooldowns.Get((Enums.PLAYER_ATTACK)i) / cd;
                    var seconds = BASE_COOLDOWN.Get(attackType) - (BASE_COOLDOWN.Get(attackType) - m_currentCooldowns.Get((Enums.PLAYER_ATTACK)i));

                    GameUIManager.UpdateCooldownDisplay((Enums.PLAYER_ATTACK)i, fillAmt, seconds);
                }
            }
        }

        /// <summary>
        /// Updates persistent data with the a new xp value and performs any relevant actions,
        /// such as animating the level up icon if a new level is hit etc.
        /// </summary>
        void UpdateXP(int experienceGained, bool isInitialising = false)
        {
            // If initialising, then the xp is being loaded to memory and does not need to be added to this unit. 
            if (!isInitialising) m_playerStateData.XP += experienceGained;

            // Save the xp value to persistent data.
            PersistentData.SaveInt(PersistentData.KEY_INT.XP, m_playerStateData.XP);

            // Calculate current level, from xp.
            var updatedLevel = LevelScaling.GetLevel(m_playerStateData.XP);

            // Calculate UI values for updating the level icon
            float currentLevelXP = LevelScaling.GetXP(updatedLevel + 1) - LevelScaling.GetXP(updatedLevel);
            var fillAmount = (m_playerStateData.XP - LevelScaling.GetXP(updatedLevel)) / currentLevelXP;

            // If a new level was reached, play animations etc. Also, cooldowns and health are reset. Otherwise just
            // update the level display with the new values.
            if (updatedLevel != m_playerStateData.Level)
            {
                GameUIManager.TriggerLevelUpAnimation();
                GameUIManager.UpdatePlayerLevel(updatedLevel, fillAmount);

                for (int i = 0; i < m_currentCooldowns.Count; i++)
                {
                    m_currentCooldowns.Set((Enums.PLAYER_ATTACK)i, 0);
                    GameUIManager.UpdateCooldownDisplay((Enums.PLAYER_ATTACK)i, 0, 0);
                }

                m_playerStateData.MaxHealth = BonusManager.GetModifiedValue(Enums.PLAYER_STAT.BOOST_HEALTH, LevelScaling.GetScaledHealth(updatedLevel, (int)m_baseHealth));
                Health = m_playerStateData.MaxHealth;
                m_healthbar.UpdateHealthDisplay(1, (int)m_playerStateData.MaxHealth);

                if (!isInitialising)
                {
                    Instantiate(m_particleLevelUp, transform.position, Quaternion.identity);

                    BonusManager.AddCredits(LevelScaling.CreditsPerLevel, true);
                }
            }
            else
            {
                GameUIManager.UpdatePlayerLevel(updatedLevel, fillAmount);
            }

            // Store the current level in memory.
            m_playerStateData.Level = updatedLevel;
        }
        #endregion 02_PRIVATE_MEMBER_FUNCTIONS ------------------------------------------------------------------------------------------------

        #region 03_OVERRIDE_MEMBER_FUNCTIONS --------------------------------------------------------------------------------------------------
        /// <summary>
        /// Called by this units base class, and is used for updating movement related behaviour such as walking animation.
        /// </summary>
        public override void OnFollowPath(float speedPercent)
        {
            if (speedPercent > 0) speedPercent = Mathf.Clamp01(speedPercent + 0.5f);

            m_animator.SetFloat("MovementBlend", 1 - speedPercent);
        }

        /// <summary>
        /// Register this to the GameManager.OnStartRun event. Add all initialisation behaviour.
        /// </summary>
        /// Note: This is done via an event, rather than the Awake function to avoid issues arising from
        /// attempting to access components that arent initialised at runtime etc.
        public override void OnStartLevel()
        {
            Platforms.RegisterPlayer(this);

            GameUIManager.Visible(true);

            PreviousPos = transform.position;

            CurrentTarget = null;

            PathingTarget = FindObjectOfType<Chest>();
            if (PathingTarget == null)
                PathingTarget = GameObject.FindGameObjectWithTag("Boss").GetComponent<EnemyPathFindingObject>() as ITargetable;

            m_playerStateData.EndTarget = PathingTarget;

            m_healthbar = FindObjectOfType<PlayerHealthBar>();

            UpdateXP(m_playerStateData.XP, true);
            m_playerStateData.MaxHealth = Health;

            m_healthbar.UpdateHealthDisplay(1, (int)m_playerStateData.MaxHealth);

            m_enemyHealthbar = FindObjectOfType<EnemyHealthBar>();

            GameUIManager.SetCurrentPlayerReference(this);

            m_playerStateData.Level = LevelScaling.GetLevel(m_playerStateData.XP);

            Running = true;

            UpdatePathTarget(PathingTarget);
            StartCoroutine(RefreshPath());

            UpdateBonusDisplay();
        }
        #endregion 03_OVERRIDE_MEMBER_FUNCTIONS -----------------------------------------------------------------------------------------------

        #region 04_MEMBER_IENUMERATORS --------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Asynchronously Applies haptic feedback and hit freeze.
        /// </summary>
        IEnumerator ApplyHapticFeedback(float duration, bool buff = false, bool vibrate = false)
        {
            // If haptic feedback is turned on, apply the freeze and vibrate the device (will only vibrate
            // if android.
            if (SettingsManager.Haptic())
            {
                Time.timeScale = 0.01f;
                float pauseEndTime = Time.realtimeSinceStartup + duration;
                while (PauseManager.IsPaused() || Time.realtimeSinceStartup < pauseEndTime)
                {
                    yield return null;
                }
                Time.timeScale = 1;
                if (vibrate) Vibration.Vibrate(Vibration.GenVibratorPattern(0.2f, 50), -1);
            }

            // If this is during a buff sequence, perform ultimate related behaviour such as triggering the
            // particle system.
            if (buff)
            {
                Sparky.ResetIntensity();
                Sparky.IncreaseIntensity();
                m_particleBuff.SetActive(true);
                AudioManager.PlayFX(Enums.SFX_TYPE.BIG_IMPACT);
            }
        }

        /// <summary>
        /// Asynchronously performs actions required after the boss is killed.
        /// </summary>
        /// <returns></returns>
        IEnumerator BossDeadSequence()
        {
            AudioManager.CrossFadeBGM(Enums.BGM_VARIATION.QUIET, 4);

            yield return new WaitForSecondsRealtime(2);

            GameManager.TriggerEndScreen();
        }

        /// <summary>
        /// Asynchronously perform ultimate features such as animations and state updates.
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        IEnumerator UltimateSequence(float duration)
        {
            Sparky.DisableLight();

            m_playerStateData.ShouldApplyUltiDamage = true;

            StartCoroutine(ApplyHapticFeedback(0.3f, true, true));

            yield return new WaitForSeconds(duration - Time.fixedDeltaTime);

            m_particleBuff.SetActive(false);

            m_baseDamage = m_playerStateData.BaseDamageHolder;

            m_playerStateData.ShouldApplyUltiDamage = false;

            // This coroutine must be started from this script, because about 75% of the time, if you call it directly on 
            // Sparky, it disappears for some reason. (The Sparky object still exists, so I think it's a bug with Unity).
            StartCoroutine(Sparky.ResetIntensityAsync(0.5f));
        }
        #endregion 04_MEMBER_IENUMERATORS -----------------------------------------------------------------------------------------------------
    }
}