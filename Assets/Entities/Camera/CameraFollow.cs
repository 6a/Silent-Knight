using UnityEngine;

/// <summary>
/// Handles camera follow behaviour
/// </summary>
public class CameraFollow : MonoBehaviour
{
    // Reference to focus point (as we want the camera to follow slightly ahead of the knights facing direction).
    JPlayerUnit m_knight;
    [SerializeField] Vector3 m_offset;

    void Start()
    {
        
    }

    void LateUpdate()
    {
        if (m_knight == null) m_knight = FindObjectOfType<JPlayerUnit>();
        else transform.position = m_knight.FocusPoint + m_offset;

    }
}
