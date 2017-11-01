using UnityEngine;

/// <summary>
/// Handles camera follow behaviour
/// </summary>
public class CameraFollow : MonoBehaviour
{
    // Editor exposed variable for the aggressiveness of the follow mechanic.
    [SerializeField] float m_smoothTime;

    // Default offset of camera from knight
    Vector3 m_offset;

    // Reference to knight script (and therefore parent gameobject etc.).
    JKnightControl m_knight;

    // Reference to focus point (as we want the camera to follow slightly ahead of the knights facing direction).
    GameObject m_target;

    void Awake ()
    {
        m_knight = FindObjectOfType<JKnightControl>();
        m_target = GameObject.Find("KnightCameraTarget");
        m_offset = transform.position - m_knight.transform.position;
    }

	void Update ()
    {
        // zero velocity because Unity docs says so.
        Vector3 velocity = Vector3.zero;

        // target is the focus point + the desired offset or the knight itself + the offset
        Vector3 target;
        if (m_knight.Moving) target = m_offset + m_target.transform.position;
        else target = m_offset + m_knight.transform.position;

        // Using SmoothDamp this gameobject will lerp towards the target position, producing a smooth
        // camera follow effect.
        transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, m_smoothTime);
	}
}
