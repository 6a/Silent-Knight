using System.Collections;
using UnityEngine;

/// <summary>
/// Represents the glowing ball object that follows the player unit around.
/// </summary>
public class Sparky : MonoBehaviour
{
    // Reference to player unit
    PlayerPathFindingObject m_player;

    // Reference to the light object attached to Sparky.
    Light m_light;

    // Sparky's vertical distance away from the origin point, for simulating bobbing.
    float m_verticalOffset;

    // State bool to check whether Sparky is currently performing a movement routine.
    public bool IsMoving { get; set; }

    static Sparky m_instance;

    // Chance that a movement routine will be triggered.
    const int TRIGGER_CHANCE = 180;

    // Base instensity for Sparkys light.
    static float m_baseIntensity;

    private void Awake()
    {
        m_player = FindObjectOfType<PlayerPathFindingObject>();
        m_light = GetComponentInChildren<Light>();
        m_verticalOffset = 0;
        transform.parent = null;

        m_instance = this;
        m_baseIntensity = m_light.intensity;
        GameManager.OnStartRun += OnStartRun;
    }

    private void OnStartRun()
    {
        IsMoving = true;
    }

    void LateUpdate ()
    {
        // Calculate Sparky's offset.
        m_verticalOffset = Mathf.Sin(Time.time * 3);

        // Modify the offset to reduce magnitute.
        var diff = Vector3.up * m_verticalOffset * 0.1f;

        // Move Sparky accordingly. In reality, the parent gameobject is manipulated, which pivots on the center of the player.
        transform.position = new Vector3(m_player.transform.position.x, 1, m_player.transform.position.z) + diff;

        // If Sparky is 'in front' of the player (in a location too close to the player's front), cancel any movement 
        // routines shift him away by 1 unit. This allows for a smooth turning transition if the player makes 
        // sudden turns. Otherwise, Sparky will begin a movement routine if a random roll is successful.
        if (IsInFront())
        {
            StopAllCoroutines();

            if (IsLeft())
            {
                transform.Rotate(transform.up, -1);
            }
            else
            {
                transform.Rotate(transform.up, 1);
            }
        }
        else
        {
            if (!IsMoving) return;

            bool willMove = (Random.Range(0, TRIGGER_CHANCE) == 0);

            if (willMove)
            {
                StopAllCoroutines();

                StartCoroutine(Rotate());
            }
        }
	}

    /// <summary>
    /// Asynchronously rotates sparky around the players position.
    /// </summary>
    IEnumerator Rotate()
    {
        // The number of movement iterations.
        int movement = Random.Range(0, 40);
        int limit = movement;

        // Determines rotation direction (clockwise/anticlockwise).
        int mod = 0;

        // If Sparky is in front of the player, determine which direction to move it to ensure that Sparky
        // moves away from the front of the player. Otherwise, choose a random direction.
        if (IsInFront())
        {
            if (IsLeft())
            {
                mod = -1;
            }
            else
            {
                mod = 1;
            }
        }
        else
        {
            mod = (Random.Range(0, 2) == 0) ? -1 : 1;
        }

        // While there are movement iterations left to process, and Sparky is in a valid position, apply 1 iteration of 
        // the desired rotation.
        while (movement > 0 && !IsInFront())
        {
            float angle = Mathf.Deg2Rad * (((float)movement / (float)limit) * 180);

            float nextMovement = 10 * Mathf.Sin(angle);

            transform.Rotate(transform.up, nextMovement * mod);

            movement--;
            yield return new WaitForFixedUpdate();
        }
    }

    /// <summary>
    /// Returns the angle between Sparky and the players facing direction.
    /// </summary>
    float GetAngle()
    {
        return Vector3.Angle((m_player.transform.position - m_light.transform.position).normalized, m_player.transform.forward);
    }

    /// <summary>
    /// Returns true if Sparky is 'in front' of the player. Returns false if Sparky is 'behind' the player.
    /// </summary>
    bool IsInFront()
    {
        const float TOLERANCE = -0.2f;

        // Uses dot product to determine Sparkys orientation in relation to the players front and back.
        return (Vector3.Dot((m_player.transform.position - m_light.transform.position).normalized, m_player.transform.forward)) < TOLERANCE;
    }

    /// <summary>
    /// Returns true if Sparky is to the left of the player's facing direction.
    /// Returns false if Sparky is to the right of the player's facing direction.
    /// </summary>
    bool IsLeft()
    {
        const float TOLERANCE = 0;
        
        // Uses cross product to determine Sparkys orientation relative to the players forward direction.
        return (Vector3.Cross((m_player.transform.position - m_light.transform.position).normalized, m_player.transform.forward)).y < TOLERANCE;
    }

    /// <summary>
    /// Increase the intensity of the light attached to Sparky, by a flat amount (3).
    /// </summary>
    public static void IncreaseIntensity()
    {
        const float INCREASE = 3;

        m_instance.m_light.intensity = m_baseIntensity + INCREASE;
    }

    /// <summary>
    /// Resets the light attached to Sparky's intensity to the default value.
    /// </summary>
    /// <param name="t"></param>
    public static void ResetIntensity(float t = 1f)
    {
        m_instance.m_light.intensity = m_baseIntensity;
    }

    /// <summary>
    /// Asynchronously, smoothly interpolates the intensity of the light attached to Sparky.
    /// </summary>
    public static IEnumerator ResetIntensityAsync(float t)
    {
        var diff = m_instance.m_light.intensity - m_baseIntensity;

        var increment = diff / (t / Time.deltaTime);

        while (m_instance.m_light.intensity > m_baseIntensity)
        {
            m_instance.m_light.intensity -= increment;
            yield return new WaitForFixedUpdate();
        }

        m_instance.m_light.intensity = m_baseIntensity;
    }

    /// <summary>
    /// Disables the light attached to Sparky.
    /// </summary>
    public static void DisableLight()
    {
        m_instance.m_light.intensity = 0;
    }

}