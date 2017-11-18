using UnityEngine;

/// <summary>
/// Handles camera follow behaviour
/// </summary>
public class CameraFollow : MonoBehaviour
{
    // Reference to focus point (as we want the camera to follow slightly ahead of the knights facing direction).
    JPlayerUnit m_knight;
    Vector3 m_offset;

    void Start()
    {
        transform.parent = null;

        m_knight = FindObjectOfType<JPlayerUnit>();

        m_offset = transform.position - m_knight.FocusPoint;
    }

    void LateUpdate()
    {
        transform.position = m_knight.FocusPoint + m_offset;
    }
}
