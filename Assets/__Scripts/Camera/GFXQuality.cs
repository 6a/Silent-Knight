using UnityEngine;
using SilentKnight.Utility;

namespace SilentKnight.CameraTools
{
    /// <summary>
    /// Handles Graphics quality settings.
    /// </summary>
    public class GFXQuality : MonoBehaviour
    {
        GFXQuality m_instance;

        Enums.GFX_QUALITY m_quality;

        void Awake()
        {
            m_instance = this;

            // Note that vsync is disabled on all levels due to mobile performance issues.
            QualitySettings.vSyncCount = 0;
        }

        void Start()
        {
            // Load the current quality setting from persistent data.
            m_quality = (Enums.GFX_QUALITY)PersistentData.LoadInt(PersistentData.KEY_INT.LEVEL_GFX);
            UpdateQuality(m_quality);
        }

        /// <summary>
        /// Sets the graphics quality.
        /// </summary>
        public static void UpdateQuality(Enums.GFX_QUALITY quality)
        {
            // Note: As well as the overall quality settings being set, a few other per-level settings are 
            // adjusted as well.

            QualitySettings.SetQualityLevel((int)quality);

            switch (quality)
            {
                case Enums.GFX_QUALITY.LOW:
                    Application.targetFrameRate = 30;
                    Screen.sleepTimeout = 30;
                    break;
                case Enums.GFX_QUALITY.MID:
                    Application.targetFrameRate = 30;
                    Screen.sleepTimeout = 60;
                    break;
                case Enums.GFX_QUALITY.HIGH:
                    Application.targetFrameRate = 60;
                    Screen.sleepTimeout = SleepTimeout.NeverSleep;
                    break;
            }
        }
    }
}