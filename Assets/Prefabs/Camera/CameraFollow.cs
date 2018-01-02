using UnityEngine;

/// <summary>
/// Handles camera follow behaviour
/// </summary>
public class CameraFollow : MonoBehaviour
{
    // Reference to focus point (as we want the camera to follow slightly ahead of the knights facing direction).
    JPlayerUnit m_knight;
    [SerializeField] Vector3 m_offset;

    static CameraFollow m_instance;

    void Awake()
    {
        m_instance = this;
    }

    void LateUpdate()
    {
        if (m_knight == null) m_knight = FindObjectOfType<JPlayerUnit>();
        else transform.position = m_knight.FocusPoint + m_offset;
    }

    public static void DereferenceKnight()
    {
        m_instance.m_knight = null;
    }
}
