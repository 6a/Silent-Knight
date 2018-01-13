using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CooldownSpinner : MonoBehaviour
{
    TextMeshProUGUI m_textObject;
    Image m_sprite;
    Color m_baseColor;
    bool m_pulsing;

    void Awake()
    {
        m_textObject = GetComponentInChildren<TextMeshProUGUI>();
        m_sprite = GetComponent<Image>();
        m_baseColor = m_sprite.color;
    }

    public float Cooldown()
    {
        return m_sprite.fillAmount;
    }

    public void UpdateRadial(float fillAmount, float remainingTime, int decimalPlaces = 0)
    {
        if (PauseManager.Paused()) return;

        m_sprite.fillAmount = fillAmount;
        if (fillAmount <= 0)
        {
            m_textObject.text = "";
        }
        else
        {
            m_textObject.text = remainingTime.ToString("F" + (decimalPlaces));
        }

    }

    public void Pulse()
    {
        if (!m_pulsing) StartCoroutine(PulseAsync(0.5f));
    }

    IEnumerator PulseAsync(float seconds)
    {
        m_pulsing = true;
        float diff = 0;
        int iterations = (int)(seconds / Time.fixedDeltaTime);
        float increment = 180.0f / iterations;

        do
        {
            diff += increment;
            var newAlpha = m_baseColor.a + (Mathf.Sin(diff * Mathf.Deg2Rad) * 0.3f);
            m_sprite.color = new Color(m_baseColor.r, m_baseColor.g, m_baseColor.b, newAlpha);
            iterations--;
            yield return new WaitForFixedUpdate();
        } while (iterations > 0);

        m_sprite.color = m_baseColor;
        m_pulsing = false;
    }
}
