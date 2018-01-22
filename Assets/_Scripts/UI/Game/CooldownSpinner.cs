using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Component helper for cooldown spinner UI elements.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI), typeof(Image))] public class CooldownSpinner : MonoBehaviour
{
    // References to attached UI objects.
    TextMeshProUGUI m_textObject;
    Image m_sprite;

    // Starting colour of the sprite being used.
    Color m_baseColor;

    // True if the pulser is currently pulsing.
    bool m_pulsing;

    void Awake()
    {
        m_textObject = GetComponentInChildren<TextMeshProUGUI>();
        m_sprite = GetComponent<Image>();
        m_baseColor = m_sprite.color;
    }

    /// <summary>
    /// Updates this cooldown spinner.
    /// </summary>
    public void UpdateSpinner(float fillAmount, float remainingTime, int decimalPlaces = 0)
    {
        // Protection against updating while the game is paused.
        if (PauseManager.IsPaused()) return;

        // Update the spinner fill.
        m_sprite.fillAmount = fillAmount;

        // If the spinner is empty, remove any cooldown text. Otherwise, update the text.
        if (fillAmount <= 0)
        {
            m_textObject.text = "";
        }
        else
        {
            // Note: string is formatted using the 'F#' formatting argument for ToString() which should
            // trim the string representation of the float value to the desired number of decimal places.
            m_textObject.text = remainingTime.ToString("F" + (decimalPlaces));
        }
    }

    /// <summary>
    /// Pulse this cooldown spinners cooldown radial.
    /// </summary>
    public void Pulse()
    {
        const float PULSE_TIME = 0.5f;

        if (!m_pulsing) StartCoroutine(PulseAsync(PULSE_TIME));
    }

    /// <summary>
    /// Asynchronously pulse this cooldown spinners cooldown radial.
    /// </summary>
    IEnumerator PulseAsync(float seconds)
    {
        // This function uses the sin function to produce an alpha value that starts at 0, rises to 1 and falls back to 0.

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
