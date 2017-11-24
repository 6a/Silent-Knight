using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] Slider m_slider;
    [SerializeField] Image m_fill;
    [SerializeField] Image m_bg;
    [SerializeField] float m_lifeTime;

    Transform m_anchor;
    RectTransform m_transform;

    float m_TimeToStartFade;

    bool m_running;

    void Awake ()
    {
        m_transform = GetComponent<RectTransform>();
    }

    void Update ()
    {
		if (m_running)
        {
            m_transform.position = Camera.main.WorldToScreenPoint(m_anchor.position);

            if (Time.time > m_TimeToStartFade)
            {
                if (m_bg.color.a == 0)
                {
                    m_running = false;
                    return;
                }

                m_bg.color = new Color(m_bg.color.r, m_bg.color.g, m_bg.color.b, m_bg.color.a - 0.01f);
                m_fill.color = new Color(m_fill.color.r, m_fill.color.g, m_fill.color.b, m_fill.color.a - 0.01f);
            }
        }
	}

    public void Init(Transform anchor)
    {
        m_anchor = anchor;
        m_transform.position = Camera.main.WorldToScreenPoint(anchor.position);
    }

    public void UpdateHealthDisplay(float value, bool silent = false)
    {
        if (!silent)
        {
            m_running = true;
            m_TimeToStartFade = Time.time + m_lifeTime;
            m_bg.color = new Color(m_bg.color.r, m_bg.color.g, m_bg.color.b, 1);
            m_fill.color = new Color(m_fill.color.r, m_fill.color.g, m_fill.color.b, 1);
        }

        m_slider.value = value;
    }

    public void Pulse(bool positiveAction)
    {
        if (positiveAction)
        {

        }
        else
        {

        }
    }
}
