using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FCT : MonoBehaviour
{
    [SerializeField] float m_speed;
    [Range(-50, 50)][SerializeField] int m_offset;

    Vector2 m_dir;
    Vector2 m_totalMovement;
    Vector3 m_worldPos;
    bool m_isFloating = false;
    Vector3 m_randomOffset;

    RectTransform m_transform;
    TextMeshProUGUI m_tmp;

    void Awake()
    {
        m_transform = GetComponent<RectTransform>();
        m_tmp = GetComponent<TextMeshProUGUI>();
    }

    public void Init(string text, Vector3 worldPos, Vector2? dir)
    {
        m_tmp.text = text;
        m_worldPos = worldPos;

        m_randomOffset = new Vector3(Random.Range(-m_offset, m_offset), Random.Range(-m_offset, m_offset), 0);

        if (dir.HasValue)
        {
            m_dir = dir.Value.normalized;
        }
        else
        {
            m_dir = m_randomOffset.normalized;
        }

        m_transform.position = Camera.main.WorldToScreenPoint(worldPos);
    }

    public void OnTriggerFloatAway()
    {
        m_isFloating = true;
    }

    public void OnEndSequence()
    {
        m_isFloating = false;
        Destroy(gameObject);
    }

    void LateUpdate()
    {
        m_transform.position = Camera.main.WorldToScreenPoint(m_worldPos) + m_randomOffset;

        if (m_isFloating)
        {
            m_totalMovement += m_dir * m_speed * Time.deltaTime;

            m_transform.position += new Vector3(m_totalMovement.x, m_totalMovement.y, 0);
        }
    }
}
