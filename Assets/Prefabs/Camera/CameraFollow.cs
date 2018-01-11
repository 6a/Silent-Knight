using System.Collections;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

/// <summary>
/// Handles camera follow behaviour
/// </summary>
public class CameraFollow : MonoBehaviour
{
    // Reference to focus point (as we want the camera to follow slightly ahead of the knights facing direction).
    JPlayerUnit m_knight;
    [SerializeField] Vector3 m_offset;
    [SerializeField] BlurOptimized m_optimizedBlur;

    static CameraFollow m_instance;

    bool m_switching;
    bool m_isInRearView;
    bool m_isInDeathView;

    void Awake()
    {
        m_instance = this;
        m_switching = false;
    }

    void LateUpdate()
    {
        if (m_switching) return;
        if (m_knight == null) m_knight = FindObjectOfType<JPlayerUnit>();
        else
        {
            if (m_isInRearView)
            {
                const float LS = 4;

                transform.position = Vector3.Lerp(transform.position, m_knight.GetReferenceTarget().position, Time.deltaTime * LS);
                var targetRotation = Quaternion.LookRotation(m_knight.GetLookTarget().position - transform.position);
                var newRotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * LS * 0.5f);
                transform.rotation = newRotation;
            }
            else if (!m_isInDeathView)
            {
                transform.position = m_knight.FocusPoint + m_offset;
            }
        }
    }

    public static void DereferenceKnight()
    {
        m_instance.m_knight = null;
    }

    Coroutine m_switchingRoutine;
    bool m_switchingFinished;

    public static bool SwitchViewRear()
    {
        var referencePos = m_instance.m_knight.GetReferenceTarget();
        var lookTarget = m_instance.m_knight.GetLookTarget();

        if (m_instance.m_switchingRoutine == null)
            m_instance.m_switchingRoutine = m_instance.StartCoroutine(m_instance.SwitchToRearView(referencePos, lookTarget));

        return m_instance.m_switchingFinished;
    }

    IEnumerator SwitchToRearView(Transform t, Transform l)
    {
        const float T = 0.5f;
        m_switching = true;
        float lerp = 0;

        Vector3 startPos = transform.position;

        while (lerp < 1)
        {
            lerp += Time.deltaTime * T;

            transform.position = Vector3.Lerp(startPos, t.position, lerp);
            var targetRotation = Quaternion.LookRotation(l.transform.position - transform.position);
            var newRotation = Quaternion.Lerp(transform.rotation, targetRotation, lerp);
            transform.rotation = newRotation;

            yield return new WaitForFixedUpdate();
        }

        transform.position = t.position;

        var t2 = Quaternion.LookRotation(l.transform.position - transform.position);
        transform.rotation = t2;

        m_offset = transform.position - m_knight.FocusPoint;

        m_switchingFinished = true;
        m_switching = false;
        m_isInRearView = true;
    }

    public static void SwitchViewDeath()
    {
        m_instance.m_switching = true;
        GameManager.FadeToBlack(true);
        m_instance.StartCoroutine(m_instance.SwitchToDeathView());
    }

    IEnumerator SwitchToDeathView()
    {
        const float LS = 0.1f;

        var cam = Camera.main.transform;

        var lerp = 0f;

        m_optimizedBlur.enabled = true;

        var startPos = transform.position;
        var startRot = transform.rotation;

        while (lerp < 1)
        {
            lerp += Time.fixedDeltaTime * LS;

            m_optimizedBlur.blurSize = (lerp * 10);

            transform.position = Vector3.Lerp(startPos, m_instance.m_knight.GetDeathAnchor().position, lerp);

            var targetRotation = Quaternion.LookRotation(m_knight.GetLookTarget().position - m_instance.m_knight.GetDeathAnchor().position);
            var newRotation = Quaternion.Lerp(startRot, targetRotation, lerp);
            transform.rotation = newRotation;

            yield return new WaitForFixedUpdate();
        }
        m_isInDeathView = true;
        m_switching = false;
    }
}
