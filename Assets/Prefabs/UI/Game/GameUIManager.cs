﻿using System.Collections;
using TMPro;
using UnityEngine;

using UnityEngine.UI;
public class GameUIManager : MonoBehaviour
{
    float m_lUltiButtonAlpha, m_rUltiButtonAlpha;
    [SerializeField] Image m_leftSpinner, m_rightSpinner;
    bool m_lButtonDown, m_rButtonDown;
    bool m_isInUltimate;
    JPlayerUnit m_currentPlayer;

    [SerializeField] CooldownSpinner[] m_cooldownSpinners = new CooldownSpinner[6];
    [SerializeField] TextMeshProUGUI m_playerLevelText, m_enemyLevelText;
    [SerializeField] Image m_playerLevelFill;
    [SerializeField] Animation m_levelUpAnimation;
    [SerializeField] GameObject [] m_panels;
 
    static GameUIManager m_instance;

    bool m_ultCharging;
    bool m_musicShiftTriggered;

    void Awake()
    {
        m_instance = this;
    }

    void Update ()
    {
        if (m_rUltiButtonAlpha > 0) m_rightSpinner.transform.Rotate(0, 0, m_rUltiButtonAlpha * 4);
        if (m_lUltiButtonAlpha > 0) m_leftSpinner.transform.Rotate(0, 0, -m_lUltiButtonAlpha * 4);

        if ((m_lUltiButtonAlpha >= 1 && m_rUltiButtonAlpha >= 1) && !m_isInUltimate) 
        {
            UltiState(true);

            StartCoroutine(SimulateKeyPress(JPlayerUnit.ATTACKS.ULTIMATE));

            Audio.BlendMusicTo(Audio.BGM.LOUD, 1);
            return;
        }

        bool ldown = false, rdown = false;

        if (m_lButtonDown && m_instance.m_cooldownSpinners[4].Cooldown() == 0)
        {
            ldown = true;
            m_lUltiButtonAlpha = Mathf.Clamp01(m_lUltiButtonAlpha + Time.deltaTime);
            m_leftSpinner.color = new Color(1, 1, 1, m_lUltiButtonAlpha);
        }

        if (m_rButtonDown && m_instance.m_cooldownSpinners[5].Cooldown() == 0)
        {
            rdown = true;
            m_rUltiButtonAlpha = Mathf.Clamp01(m_rUltiButtonAlpha + Time.deltaTime);
            m_rightSpinner.color = new Color(1, 1, 1, m_rUltiButtonAlpha);
        }

        if (ldown || rdown) m_ultCharging = true;

        if (!m_musicShiftTriggered && m_ultCharging)
        {
            m_musicShiftTriggered = true;

            Audio.BlendMusicTo(Audio.BGM.LOUD, 2, true);
        }
    }

    public IEnumerator SimulateKeyPress(JPlayerUnit.ATTACKS type)
    {
        m_instance.m_currentPlayer.SimulateInputPress(type);

        yield return new WaitForEndOfFrame();

        m_instance.m_currentPlayer.SimulateInputRelease(type);
    }

    public void OnUltiButtonDown(bool left)
    {
        if (left)
        {
            m_lButtonDown = true;
        }
        else
        {
            m_rButtonDown = true;
        }

        if (FindObjectOfType<JPlayerUnit>().UltIsOnCooldown())
        {
            StartCoroutine(SimulateKeyPress(JPlayerUnit.ATTACKS.ULTIMATE));
        }
    }

    public void OnUltiButtonUp(bool left)
    {
        if (left)
        {
            m_lButtonDown = false;

            if (m_instance.m_cooldownSpinners[4].Cooldown() == 0)
            {
                DisableUltiSpinner(true); 
            }
        }
        else
        {
            m_rButtonDown = false;

            if (m_instance.m_cooldownSpinners[5].Cooldown() == 0)
            {
                DisableUltiSpinner(false);
            }
        }

        if (!m_isInUltimate)
        {
            Audio.BlendMusicTo(Audio.BGM.QUIET, 1);
            m_musicShiftTriggered = false;
            m_ultCharging = false;
        }
    }

    public static void UltiState(bool state)
    {
        m_instance.m_isInUltimate = state;

        if (!state)
        {
            m_instance.DisableUltiSpinner(true);
            m_instance.DisableUltiSpinner(false);
        }
    }

    void DisableUltiSpinner(bool left)
    {
        if (left)
        {
            m_leftSpinner.color = new Color(1, 1, 1, 0);
            m_lUltiButtonAlpha = 0;
        }
        else
        {
            m_rightSpinner.color = new Color(1, 1, 1, 0);
            m_rUltiButtonAlpha = 0;
        }
    }

    public static void SetCurrentPlayerReference(JPlayerUnit player)
    {
        m_instance.m_currentPlayer = player;
    }

    public void OnSimpleButtonDown(int button)
    {
        StartCoroutine(SimulateKeyPress((JPlayerUnit.ATTACKS)button));
    }

    public static void UpdateSpinner(JPlayerUnit.ATTACKS spinner, float fillAmount, float remainingTime, int decimalPlaces = 0)
    {
        if ((int)spinner >= (int)JPlayerUnit.ATTACKS.ULTIMATE)
        {
            m_instance.m_cooldownSpinners[(int)JPlayerUnit.ATTACKS.ULTIMATE].UpdateRadial(fillAmount, remainingTime, decimalPlaces);
            m_instance.m_cooldownSpinners[(int)JPlayerUnit.ATTACKS.ULTIMATE + 1].UpdateRadial(fillAmount, remainingTime, decimalPlaces);
        }
        else
        {
            m_instance.m_cooldownSpinners[(int)spinner].UpdateRadial(fillAmount, remainingTime, decimalPlaces);
        }
    }

    public static void Pulse (JPlayerUnit.ATTACKS spinner)
    {
        if ((int)spinner >= (int)JPlayerUnit.ATTACKS.ULTIMATE)
        {
            m_instance.m_cooldownSpinners[(int)JPlayerUnit.ATTACKS.ULTIMATE].Pulse();
            m_instance.m_cooldownSpinners[(int)JPlayerUnit.ATTACKS.ULTIMATE + 1].Pulse();
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
        UltiState(false);

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

    public static void ResetAudioTrigger()
    {
       m_instance.m_musicShiftTriggered = false;
       m_instance.m_ultCharging = false;
    }
}
