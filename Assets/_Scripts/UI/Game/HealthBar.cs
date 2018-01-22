using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Healthbar helper component. Handles filling of the healthbar and text overlay.
/// </summary>
public class HealthBar : MonoBehaviour
{
    // Rect transform of the health bar fill object.
    [SerializeField] RectTransform m_slider;

    // Width of the fill area, in UI pixels.
    [SerializeField] int m_width;

    // The text component that shows health numbers.
    [SerializeField] Text m_text;

    // Helper string for formatting health text (X / X).
    const string FORMAT_TEMPLATE = " / ";

    /// <summary>
    /// Updates the healthbar UI object. Takes a value as a number between 0-1.
    /// </summary>
    public void UpdateHealthDisplay(float value, int maxHealth)
    {
        m_slider.sizeDelta = new Vector2(value * m_width , m_slider.sizeDelta.y);
        m_text.text = (maxHealth * value).ToString("F0") + FORMAT_TEMPLATE + maxHealth.ToString("F0");
    }
}
