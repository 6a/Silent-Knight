using UnityEngine;

/// <summary>
/// Handles camera follow behaviour
/// </summary>
public class CameraFollow : MonoBehaviour
{
    // Editor exposed variable for the aggressiveness of the follow mechanic.
    [SerializeField] float m_smoothTime;

    // Reference to focus point (as we want the camera to follow slightly ahead of the knights facing direction).
    JKnightControl m_knight;

    void Start ()
    {
        m_knight = FindObjectOfType<JKnightControl>();
        transform.parent = null;
    }

	void Update ()
    {
        // zero velocity because Unity docs says so.
        Vector3 velocity = Vector3.zero;

        var focus = m_knight.FocusPoint;

        focus.x -= 1f;
        focus.y += 3;
        focus.z -= 1f;

        // Using SmoothDamp this gameobject will lerp towards the target position, producing a smooth
        // camera follow effect.
        transform.position = Vector3.SmoothDamp(transform.position, focus, ref velocity, m_smoothTime);
	}
}
