using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public struct AudioSet
{
    Audio.FX Type;
    List<AudioClip> Clips;
    public AudioClip Random
    {
        get
        {
            var ran = UnityEngine.Random.Range(0, Clips.Count);
            return Clips[ran];
        }

    }
}

public class Audio : MonoBehaviour
{
    public enum AUDIO { MASTER, BGM, FX }
    public enum FX { SWORD_IMPACT }
    public enum BGM { QUIET, LOUD }

    [SerializeField] AudioSource m_sourceBGMQ, m_sourceBGML, m_sourceFX;
    [SerializeField] AudioMixer m_master;
    [SerializeField] bool m_testBlendUp, m_testBlendDown;

    const float AUDIO_MIN = -80f;
    const float AUDIO_MAX = 0f;
    const float AUDIO_RANGE = 80f;

    List<AudioSet> m_lib;
    Coroutine m_blendRoutine;

    static Audio m_instance;

	void Awake ()
    {
        m_instance = this;
        m_testBlendUp = false;
        m_testBlendDown = false;


    }
    void Start()
    {
        SetVolume(AUDIO.BGM, PPM.LoadFloat(PPM.KEY_FLOAT.VOL_BGM));
        SetVolume(AUDIO.FX, PPM.LoadFloat(PPM.KEY_FLOAT.VOL_FX));
        SetVolume(AUDIO.MASTER, PPM.LoadFloat(PPM.KEY_FLOAT.VOL_MASTER));
    }

    void Update()
    {
        BlendTest();
    }

    private void BlendTest()
    {
        if (m_testBlendUp)
        {
            m_testBlendUp = false;

            BlendMusicTo(BGM.LOUD, 1);
        }

        if (m_testBlendDown)
        {
            m_testBlendDown = false;

            BlendMusicTo(BGM.QUIET, 1);
        }
    }

    void BlendBGM(float amt)
    {
        var volq = 1 - amt;
        var voll = amt;

        m_sourceBGMQ.volume = volq;
        m_sourceBGML.volume = voll;
    }

    public static void BlendMusicTo(BGM to, float time = 0.5f)
    {
        if (m_instance.m_blendRoutine != null) m_instance.StopCoroutine(m_instance.m_blendRoutine);

        m_instance.m_blendRoutine = m_instance.StartCoroutine(m_instance.BlendMusicAsync(to, time));
    }

    IEnumerator BlendMusicAsync(BGM to, float t)
    {
        const float STEP = 0.02f;

        var iterations = Mathf.CeilToInt(t / STEP);

        var increments = (1f / iterations);

        bool esc = false;

        while (iterations > 0 && !esc)
        {
            if (to == BGM.LOUD)
            {
                m_sourceBGMQ.volume -= increments;
                m_sourceBGML.volume += increments;
            }
            else
            {
                m_sourceBGMQ.volume += increments;
                m_sourceBGML.volume -= increments;
            }

            if (m_sourceBGMQ.volume == 0 || m_sourceBGMQ.volume == 1) esc = true;

            iterations--;
            yield return new WaitForSecondsRealtime(STEP);
        }

        if (to == BGM.LOUD)
        {
            m_sourceBGML.volume = 1;
            m_sourceBGMQ.volume = 0;
        }
        else
        {
            m_sourceBGMQ.volume = 1;
            m_sourceBGML.volume = 0;
        }

    }

    // Set volume, range is 0 - 1 (0% > 100%)
    public static void SetVolume(AUDIO type, float vol)
    {
        switch (type)
        {
            case AUDIO.MASTER:
                var mvol = m_instance.LinearToDecibel(vol);
                m_instance.m_master.SetFloat("MasterVolume", mvol);
                break;
            case AUDIO.BGM:
                var bvol = m_instance.LinearToDecibel(vol);
                m_instance.m_master.SetFloat("BGMVolume", bvol);
                break;
            case AUDIO.FX:
                m_instance.m_sourceFX.volume = Mathf.Clamp01(vol);
                break;
        }
    }

    public static void PlayFX(FX type, Vector3? position = null)
    { 
        var clip = m_instance.m_lib[(int)type].Random;

        if (position.HasValue)
        {
            AudioSource.PlayClipAtPoint(clip, position.Value, m_instance.m_sourceFX.volume);
        }
        else
        {
            m_instance.m_sourceFX.Play();
        }
    }

    public static void StopFX(bool killAll)
    {
        m_instance.m_sourceFX.Stop();

        if (killAll)
        {
            m_instance.m_sourceFX.enabled = false;
            m_instance.m_sourceFX.enabled = true;
        }
    }

    // Credit to Liandur on unity answers
    // https://answers.unity.com/answers/1107168/view.html
    private float LinearToDecibel(float linear)
    {
        float dB;

        if (linear != 0)
            dB = 20.0f * Mathf.Log10(linear);
        else
            dB = -144.0f;

        return dB;
    }
}
