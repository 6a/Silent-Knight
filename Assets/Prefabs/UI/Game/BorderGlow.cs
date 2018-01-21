using UnityEngine.UI;
using UnityEngine;
using System.Collections;

/// <summary>
/// Helper class for pulsing a glowing edge around the edge of the screen.
/// </summary>
[RequireComponent(typeof(Image))]
public class BorderGlow : MonoBehaviour
{
    // The attached Image compnent that will be animated.
    Image m_image;

    // Reference to the current coroutine, used to prevent multiple coroutine instances (and
    // to allow for cancelling/overwriting of the current coroutine.
    Coroutine m_currentPulseCoroutine;

    static BorderGlow m_instance;

    void Awake()
    {
        m_instance = this;
        m_image = GetComponent<Image>();
        m_currentPulseCoroutine = null;
    }

    /// <summary>
    /// Pulse the current pulse image, with a certain duration and colour.
    /// </summary>
    public static void Pulse(float seconds, Color col)
    {
        m_instance.m_image.color = new Color(col.r, col.g, col.b, 0);

        if (m_instance.m_currentPulseCoroutine != null) m_instance.StopCoroutine(m_instance.m_currentPulseCoroutine);

        m_instance.m_currentPulseCoroutine = m_instance.StartCoroutine(m_instance.PulseAsync(seconds));
    }

    /// <summary>
    /// Asynchronous pulsing coroutine.
    /// </summary>
    IEnumerator PulseAsync(float seconds)
    {
        // This function uses the sin function to produce an alpha value that starts at 0, rises to 1 and falls back to 0.

        float diff = 0;
        int iterations = (int)(seconds / Time.fixedDeltaTime);
        float increment = 180.0f / iterations;

        do
        {
            diff += increment;
            var newAlpha = 0.5f * (Mathf.Sin(diff * Mathf.Deg2Rad));
            m_image.color = new Color(m_image.color.r, m_image.color.g, m_image.color.b, newAlpha);
            iterations--;
            yield return new WaitForFixedUpdate();
        } while (iterations > 0);

        m_image.color = new Color(m_image.color.r, m_image.color.g, m_image.color.b, 0);
    }
}
