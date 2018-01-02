using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public class BorderGlow : MonoBehaviour
{
    static BorderGlow m_instance;
    Image m_image;
    Coroutine m_currentPulseCoroutine;

    void Awake()
    {
        m_instance = this;
        m_image = GetComponent<Image>();
        m_currentPulseCoroutine = null;
    }

    public static void Pulse(float seconds, Color color)
    {
        m_instance.m_image.color = new Color(color.r, color.g, color.b, 0);

        if (m_instance.m_currentPulseCoroutine != null) m_instance.StopCoroutine(m_instance.m_currentPulseCoroutine);

        m_instance.m_currentPulseCoroutine = m_instance.StartCoroutine(m_instance.PulseAsync(seconds));
    }

    IEnumerator PulseAsync(float seconds)
    {
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
