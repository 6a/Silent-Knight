using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] RectTransform m_slider;
    [SerializeField] int m_width;
    [SerializeField] Text m_text;

    const string FORMAT_TEMPLATE = " / ";

    void Awake ()
    {

    }

    void Update ()
    {

	}

    public void UpdateHealthDisplay(float value, int maxHealth)
    {
        m_slider.sizeDelta = new Vector2(value * m_width , m_slider.sizeDelta.y);
        m_text.text = (int)(maxHealth * value) + FORMAT_TEMPLATE + maxHealth;
    }

    public void Pulse(bool positiveAction)
    {

    }
}
