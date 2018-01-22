using UnityEngine;

/// <summary>
/// Helper component to allow any object to flash, as long as it has the appropriate animator.
/// </summary>
[RequireComponent(typeof(Animator))]
public class FlashButton : MonoBehaviour
{
    [SerializeField] GameObject m_flasher;

    /// <summary>
    /// Initiates the "Flash" animation on the attached animator.
    /// </summary>
    public void Flash()
    {
        m_flasher.GetComponent<Animator>().SetTrigger("Flash");
    }
}
