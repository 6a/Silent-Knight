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
    [SerializeField] float m_barTime;

    const float AUDIO_MIN = -80f;
    const float AUDIO_MAX = 0f;
    const float AUDIO_RANGE = 80f;

    List<AudioSet> m_lib;
    Coroutine m_blendRoutine;

    int m_samplesPerSecond;
    int m_samplesPerBeat;
    
    static Audio m_instance;

	void Awake ()
    {
        if (m_instance)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        m_instance = this;
        m_testBlendUp = false;
        m_testBlendDown = false;
    }
    void Start()
    {
        SetVolume(AUDIO.BGM, PPM.LoadFloat(PPM.KEY_FLOAT.VOL_BGM));
        SetVolume(AUDIO.FX, PPM.LoadFloat(PPM.KEY_FLOAT.VOL_FX));
        SetVolume(AUDIO.MASTER, PPM.LoadFloat(PPM.KEY_FLOAT.VOL_MASTER));

        m_samplesPerSecond = m_sourceBGMQ.clip.frequency;

        m_samplesPerBeat = (int)((m_barTime * m_samplesPerSecond) / 4);
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

    public static void SnapMusicBlend(BGM to)
    {
        if (m_instance.m_blendRoutine != null) m_instance.StopCoroutine(m_instance.m_blendRoutine);

        if (to == BGM.LOUD)
        {
            m_instance.m_sourceBGML.volume = 1f;
            m_instance.m_sourceBGMQ.volume = 0f;
        }
        else
        {
            m_instance.m_sourceBGML.volume = 0f;
            m_instance.m_sourceBGMQ.volume = 1f;
        }
    }

    public static void BlendMusicTo(BGM to, int beats = 2, bool half = false)
    {

        if (m_instance.m_blendRoutine != null) m_instance.StopCoroutine(m_instance.m_blendRoutine);

        m_instance.m_blendRoutine = m_instance.StartCoroutine(m_instance.BlendMusicAsync(to, beats, half));
    }

    IEnumerator BlendMusicAsync(BGM to, int beats, bool half)
    {
        const float STEP = 0.02f;

        var headPos = m_sourceBGMQ.timeSamples;

        var playPos = (headPos % m_samplesPerBeat) + m_samplesPerBeat;

        while (m_sourceBGMQ.timeSamples < playPos)
        {
            yield return null;
        }

        var iterations = Mathf.CeilToInt((beats * m_samplesPerBeat / (float)m_samplesPerSecond) / STEP);

        var targetBlend = (half) ? 0.5f : 1f;

        var increments = (targetBlend / iterations);

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

        if (half)
        {
            if (to == BGM.LOUD)
            {
                m_sourceBGML.volume = 0.5f;
                m_sourceBGMQ.volume = 0.5f;
            }
            else
            {
                m_sourceBGMQ.volume = 0.5f;
                m_sourceBGML.volume = 0.5f;
            }
        }
        else
        {
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
