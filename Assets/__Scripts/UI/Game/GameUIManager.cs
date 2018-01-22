using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SilentKnight.Entities;
using SilentKnight.Utility;
using SilentKnight.Localisation;

namespace SilentKnight.UI.Game
{
    /// <summary>
    /// Handles all UI behaviour for the in-game overlay section of the UI.
    /// </summary>
    public class GameUIManager : MonoBehaviour
    {
        // Reference to the player.
        PlayerPathFindingObject m_currentPlayer;

        // Exposed references to various UI components.
        // References to the cooldown spinners. must be in order.
        [Tooltip("ORDER: Spin, kick, stun, reflect, ult left, ult right")]
        [SerializeField]
        CooldownSpinner[] m_cooldownSpinners = new CooldownSpinner[6];

        // Text boxes.
        [SerializeField] TextMeshProUGUI m_playerLevelText;

        // Player level icon fill sprite.
        [SerializeField] Image m_playerLevelFill;

        // Animator for the level up animator.
        [SerializeField] Animation m_levelUpAnimation;

        // Game UI GameObjects references.
        [SerializeField] GameObject[] m_panels;

        // Reference to enemy healthbar component class.
        [SerializeField] EnemyHealthBar m_enemyHealthBar;

        // Reference to enemy name text field component class.
        [SerializeField] EnemyTitleTextField m_enemyNameField;

        // State booleans for the left and right ultimate buttons.
        bool m_leftUltButtonDown, m_rightUltButtonDown;

        static GameUIManager m_instance;

        void Awake()
        {
            m_instance = this;
        }

        /// <summary>
        /// Public function for button press (left ultimate button.
        /// </summary>
        public void OnLeftUltButton(bool down)
        {
            m_leftUltButtonDown = down;

            if (down)
            {
                if (m_currentPlayer.UltIsOnCooldown() || (m_rightUltButtonDown && m_leftUltButtonDown))
                {
                    StartCoroutine(SimulateKeyPress(Enums.PLAYER_ATTACK.ULTIMATE));
                }
            }
        }

        /// <summary>
        /// Public function for button press (right ultimate button.
        /// </summary>
        public void OnRightUltButton(bool down)
        {
            m_rightUltButtonDown = down;

            if (down)
            {
                if (m_currentPlayer.UltIsOnCooldown() || (m_rightUltButtonDown && m_leftUltButtonDown))
                {
                    StartCoroutine(SimulateKeyPress(Enums.PLAYER_ATTACK.ULTIMATE));
                }
            }
        }

        /// <summary>
        /// Update the internal reference to the player.
        /// </summary>
        public static void SetCurrentPlayerReference(PlayerPathFindingObject player)
        {
            m_instance.m_currentPlayer = player;
        }

        /// <summary>
        /// Public function for standard button presses, that will simulate a particular key press. Argument is cast to PLAYER_ATTCK enum.
        /// </summary>
        public void OnSimpleButtonDown(int button)
        {
            StartCoroutine(SimulateKeyPress((Enums.PLAYER_ATTACK)button));
        }

        /// <summary>
        /// Asynchronously press, then release a virtual key (waits for 1 frame between presses).
        /// </summary>
        public IEnumerator SimulateKeyPress(Enums.PLAYER_ATTACK type)
        {
            m_instance.m_currentPlayer.SimulateInputPress(type);

            yield return new WaitForEndOfFrame();

            m_instance.m_currentPlayer.SimulateInputRelease(type);
        }

        /// <summary>
        /// Updates a specific cooldown display.
        /// </summary>
        public static void UpdateCooldownDisplay(Enums.PLAYER_ATTACK spinner, float fillAmount, float remainingTime, int decimalPlaces = 0)
        {
            if ((int)spinner >= (int)Enums.PLAYER_ATTACK.ULTIMATE)
            {
                m_instance.m_cooldownSpinners[(int)Enums.PLAYER_ATTACK.ULTIMATE].UpdateSpinner(fillAmount, remainingTime, decimalPlaces);
                m_instance.m_cooldownSpinners[(int)Enums.PLAYER_ATTACK.ULTIMATE + 1].UpdateSpinner(fillAmount, remainingTime, decimalPlaces);
            }
            else
            {
                m_instance.m_cooldownSpinners[(int)spinner].UpdateSpinner(fillAmount, remainingTime, decimalPlaces);
            }
        }

        /// <summary>
        /// Returns a reference to the enemy name text field.
        /// </summary>
        public static EnemyTitleTextField GetEnemyNameField()
        {
            return m_instance.m_enemyNameField;
        }

        /// <summary>
        /// returns a reference to the enemy health bar object.
        /// </summary>
        public static EnemyHealthBar GetEnemyHealthBarReference()
        {
            return m_instance.m_enemyHealthBar;
        }

        /// <summary>
        /// Pulses a specific cooldown spinner.
        /// </summary>
        public static void Pulse(Enums.PLAYER_ATTACK spinner)
        {
            Color PULSE_COLOUR = new Color(85f / 255f, 220f / 255f, 1);     // This cant be const for some reason.

            if ((int)spinner >= (int)Enums.PLAYER_ATTACK.ULTIMATE)
            {
                m_instance.m_cooldownSpinners[(int)Enums.PLAYER_ATTACK.ULTIMATE].Pulse();
                m_instance.m_cooldownSpinners[(int)Enums.PLAYER_ATTACK.ULTIMATE + 1].Pulse();
            }
            else
            {
                m_instance.m_cooldownSpinners[(int)spinner].Pulse();
            }

            BorderGlow.Pulse(0.5f, PULSE_COLOUR);
        }

        /// <summary>
        /// Updates the player level icon.
        /// </summary>
        public static void UpdatePlayerLevel(int level, float fillAmount)
        {
            m_instance.m_playerLevelText.text = level.ToString();
            m_instance.m_playerLevelFill.fillAmount = fillAmount;
        }

        /// <summary>
        /// Triggers the level up animation for the players level icon.
        /// </summary>
        public static void TriggerLevelUpAnimation()
        {
            m_instance.m_levelUpAnimation.Play();
        }

        /// <summary>
        /// Reset all cooldown spinners to show no cooldown.
        /// </summary>
        public static void Reset()
        {
            for (int i = 0; i < m_instance.m_cooldownSpinners.Length; i++)
            {
                m_instance.m_cooldownSpinners[i].UpdateSpinner(0, 0);
            }
        }

        // Toggle visibility for the Game UI components.
        public static void Visible(bool enable)
        {
            foreach (var panel in m_instance.m_panels)
            {
                panel.SetActive(enable);
            }
        }
    }
}