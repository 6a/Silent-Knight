using Localisation;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

using UnityEngine.UI;
public class GameUIManager : MonoBehaviour
{
    PlayerPathFindingObject m_currentPlayer;

    [SerializeField] CooldownSpinner[] m_cooldownSpinners = new CooldownSpinner[6];
    [SerializeField] TextMeshProUGUI m_playerLevelText, m_enemyLevelText;
    [SerializeField] Image m_playerLevelFill;
    [SerializeField] Animation m_levelUpAnimation;
    [SerializeField] GameObject [] m_panels;
    [SerializeField] EnemyHealthBar m_enemyHealthBar;
    [SerializeField] EnemyTitleTextField m_enemyTitleTextField;

    bool m_leftUltButtonDown, m_rightUltButtonDown;

    static GameUIManager m_instance;

    void Awake()
    {
        m_instance = this;
    }

    void Update ()
    {

    }

    public IEnumerator SimulateKeyPress(Enums.PLAYER_ATTACK type)
    {
        m_instance.m_currentPlayer.SimulateInputPress(type);

        yield return new WaitForEndOfFrame();

        m_instance.m_currentPlayer.SimulateInputRelease(type);
    }

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
    
    public static void SetCurrentPlayerReference(PlayerPathFindingObject player)
    {
        m_instance.m_currentPlayer = player;
    }

    public void OnSimpleButtonDown(int button)
    {
        StartCoroutine(SimulateKeyPress((Enums.PLAYER_ATTACK)button));
    }

    public static void UpdateCooldownDisplay(Enums.PLAYER_ATTACK spinner, float fillAmount, float remainingTime, int decimalPlaces = 0)
    {
        if ((int)spinner >= (int)Enums.PLAYER_ATTACK.ULTIMATE)
        {
            m_instance.m_cooldownSpinners[(int)Enums.PLAYER_ATTACK.ULTIMATE].UpdateRadial(fillAmount, remainingTime, decimalPlaces);
            m_instance.m_cooldownSpinners[(int)Enums.PLAYER_ATTACK.ULTIMATE + 1].UpdateRadial(fillAmount, remainingTime, decimalPlaces);
        }
        else
        {
            m_instance.m_cooldownSpinners[(int)spinner].UpdateRadial(fillAmount, remainingTime, decimalPlaces);
        }
    }

    public static EnemyTitleTextField GetEnemyTextField()
    {
        return m_instance.m_enemyTitleTextField;
    }

    public static EnemyHealthBar GetEnemyHealthBarReference()
    {
        return m_instance.m_enemyHealthBar;
    }

    public static void Pulse (Enums.PLAYER_ATTACK spinner)
    {
        if ((int)spinner >= (int)Enums.PLAYER_ATTACK.ULTIMATE)
        {
            m_instance.m_cooldownSpinners[(int)Enums.PLAYER_ATTACK.ULTIMATE].Pulse();
            m_instance.m_cooldownSpinners[(int)Enums.PLAYER_ATTACK.ULTIMATE + 1].Pulse();
        }
        else
        {
            m_instance.m_cooldownSpinners[(int)spinner].Pulse();
        }

        BorderGlow.Pulse(0.5f, new Color(85f/255f, 220f/255f, 1));
    }

    public static void UpdatePlayerLevel(int level, float fillAmount)
    {
        m_instance.m_playerLevelText.text = level.ToString();
        m_instance.m_playerLevelFill.fillAmount = fillAmount;
    }
    
    public static void TriggerLevelUpAnimation()
    {
        m_instance.m_levelUpAnimation.Play();
    }

    public static void UpdateEnemyLevel(int level) { m_instance.m_enemyLevelText.text = level.ToString(); }

    public static void Reset()
    {
        for (int i = 0; i < m_instance.m_cooldownSpinners.Length; i++)
        {
            m_instance.m_cooldownSpinners[i].UpdateRadial(0, 0);
        }
    }

    public static void Show(bool enable)
    {
        foreach (var panel in m_instance.m_panels)
        {
            panel.SetActive(enable);
        }
    }
}
