using UnityEngine;
/// <summary>
/// Dummy class to generate vibration permissions on Android.
/// </summary>
public class VibrationDummy : MonoBehaviour
{
    /// <summary>
    /// Dummy function to stimulate Android manifest vibration permissions.
    /// </summary>
    static void Dummy()
    {
        Handheld.Vibrate();
    }
}
