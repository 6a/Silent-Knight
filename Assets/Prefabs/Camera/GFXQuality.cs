using UnityEngine;
using UnityEngine.PostProcessing;

public class GFXQuality : MonoBehaviour
{
    public enum GQUALITY { LOW, MID, HIGH };

    GFXQuality m_instance;

    GQUALITY m_quality;

    void Awake()
    {
        m_instance = this;
        QualitySettings.vSyncCount = 0;
    }

    void Start()
    {
        m_quality = (GQUALITY)PPM.LoadInt(PPM.KEY_INT.LEVEL_GFX);
        UpdateQuality(m_quality);
    }

    public static void UpdateQuality(GQUALITY q)
    {
        print("!");
        QualitySettings.SetQualityLevel((int)q);

        switch (q)
        {
            case GQUALITY.LOW:
                Application.targetFrameRate = 30;
                Screen.sleepTimeout = 30;
                break;
            case GQUALITY.MID:
                Application.targetFrameRate = 30;
                Screen.sleepTimeout = 60;
                break;
            case GQUALITY.HIGH:
                Application.targetFrameRate = 60;
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
                break;

        }
    }
}
