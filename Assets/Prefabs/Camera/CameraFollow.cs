using System.Collections;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

/// <summary>
/// Handles camera follow behaviour.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    // Default camera offset for locking camera in fixed position.
    [Tooltip("Default camera offset")] [SerializeField] Vector3 m_offset;

    // Reference to player unit.
    PlayerPathFindingObject m_playerUnit;

    // Reference to the attached Optimized Blur component (used for on-death event).
    BlurOptimized m_optimizedBlur;

    // Reference to the current switching routine.
    Coroutine m_switchingRoutine;

    static CameraFollow m_instance;

    // State variables.
    bool m_switching;
    bool m_isInRearView;
    bool m_isInDeathView;
    bool m_switchingFinished;

    void Awake()
    {
        m_instance = this;
        m_switching = false;
        m_optimizedBlur = GetComponentInChildren<BlurOptimized>();
    }

    void LateUpdate()
    {
        // Early exit when a camera shift is taking place.
        if (m_switching) return;

        // Find the player unit if the reference is lost (such as after a level load). Otherwise act accordingly.
        if (m_playerUnit == null) m_playerUnit = FindObjectOfType<PlayerPathFindingObject>();
        else
        {
            if (m_isInRearView)
            {
                const float LS = 4;

                // If the rear view is active, perform the following actions to smooth-lock the camera behind the player unit.
                transform.position = Vector3.Lerp(transform.position, m_playerUnit.GetReferenceTarget().position, Time.deltaTime * LS);
                var targetRotation = Quaternion.LookRotation(m_playerUnit.GetLookTarget().position - transform.position);
                var newRotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * LS * 0.5f);
                transform.rotation = newRotation;
            }
            else if (!m_isInDeathView)
            {
                // Move the camera to the offset position to keep it locked in place.
                transform.position = m_playerUnit.FocusPoint + m_offset;
            }
        }
    }

    /// <summary>
    /// Remove the internal reference to thep player unit. Call when the player unit is destroyed during level loading.
    /// </summary>
    public static void DereferencePlayerUnit()
    {
        m_instance.m_playerUnit = null;
    }

    /// <summary>
    /// Smoothly transition the camera to rear view mode. Returns false while the transition is taking place, and will not
    /// overwrite the current routine if it is already running.
    /// </summary>
    public static bool SwitchViewRear()
    {
        var referencePos = m_instance.m_playerUnit.GetReferenceTarget();
        var lookTarget = m_instance.m_playerUnit.GetLookTarget();

        if (m_instance.m_switchingRoutine == null)
            m_instance.m_switchingRoutine = m_instance.StartCoroutine(m_instance.SwitchViewRearAsync(referencePos, lookTarget));

        return m_instance.m_switchingFinished;
    }

    // Smooth lerps the camera to rear view.
    IEnumerator SwitchViewRearAsync(Transform t, Transform l)
    {
        const float TIME_MOD = 0.5f;
        m_switching = true;
        float lerp = 0;

        Vector3 startPos = transform.position;

        while (lerp < 1)
        {
            lerp += Time.deltaTime * TIME_MOD;

            transform.position = Vector3.Lerp(startPos, t.position, lerp);
            var targetRotation = Quaternion.LookRotation(l.transform.position - transform.position);
            var newRotation = Quaternion.Lerp(transform.rotation, targetRotation, lerp);
            transform.rotation = newRotation;

            yield return new WaitForFixedUpdate();
        }

        transform.position = t.position;

        var t2 = Quaternion.LookRotation(l.transform.position - transform.position);
        transform.rotation = t2;

        m_offset = transform.position - m_playerUnit.FocusPoint;

        m_switchingFinished = true;
        m_switching = false;
        m_isInRearView = true;
    }

    /// <summary>
    /// Smoothly transition the camera to death-view mode.
    /// </summary>
    public static void SwitchViewDeath()
    {
        m_instance.m_switching = true;
        GameManager.FadeToBlack(true);
        m_instance.StartCoroutine(m_instance.SwitchViewDeathAsync());
    }

    IEnumerator SwitchViewDeathAsync()
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

            transform.position = Vector3.Lerp(startPos, m_instance.m_playerUnit.GetDeathAnchor().position, lerp);

            var targetRotation = Quaternion.LookRotation(m_playerUnit.GetLookTarget().position - m_instance.m_playerUnit.GetDeathAnchor().position);
            var newRotation = Quaternion.Lerp(startRot, targetRotation, lerp);
            transform.rotation = newRotation;

            yield return new WaitForFixedUpdate();
        }
        m_isInDeathView = true;
        m_switching = false;
    }
}
