using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public struct AudioSet
{
    const string m_root = "Audio/FX/";

    public Audio.FX Type;
    public string File;
    public int Count;
    public AudioClip Random
    {
        get
        {
            var ran = UnityEngine.Random.Range(1, Count + 1);
            AudioClip c = Resources.Load(m_root + File + ran) as AudioClip;
            return c;
        }

    }
}

public class Audio : MonoBehaviour
{
    public enum AUDIO { MASTER, BGM, FX }
    public enum FX { SWORD_IMPACT, FOOTSTEP, ENEMY_ATTACK_IMPACT, KICK, SHIELD_SLAM, DEFLECT, LEVEL_CHIME }
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

        m_lib = new List<AudioSet>();

        // load lib to memory
        AudioSet aSet = new AudioSet()
        {
            Type = FX.SWORD_IMPACT,
            File = "player_sword/hit",
            Count = 5
        };
        m_lib.Add(aSet);

        aSet = new AudioSet()
        {
            Type = FX.FOOTSTEP,
            File = "steps/step",
            Count = 2
        };
        m_lib.Add(aSet);

        aSet = new AudioSet()
        {
            Type = FX.ENEMY_ATTACK_IMPACT,
            File = "enemy_attack_hit/HIT",
            Count = 5
        };
        m_lib.Add(aSet);

        aSet = new AudioSet()
        {
            Type = FX.KICK,
            File = "kick/kick",
            Count = 1
        };
        m_lib.Add(aSet);

        aSet = new AudioSet()
        {
            Type = FX.SHIELD_SLAM,
            File = "shield_slam/slam",
            Count = 1
        };
        m_lib.Add(aSet);

        aSet = new AudioSet()
        {
            Type = FX.DEFLECT,
            File = "deflect/deflect",
            Count = 3
        };
        m_lib.Add(aSet);

        aSet = new AudioSet()
        {
            Type = FX.LEVEL_CHIME,
            File = "bells/level_chime",
            Count = 1
        };
        m_lib.Add(aSet);

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

        BlendMusicTo(BGM.QUIET, 2);
    }

    void Update()
    {
        BlendTest();

        if (m_sourceBGML.timeSamples != m_sourceBGMQ.timeSamples)
        {
            m_sourceBGML.timeSamples = m_sourceBGMQ.timeSamples;
        }

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
            m_instance.m_sourceFX.clip = clip;
            m_instance.m_sourceFX.Play();
        }
    }

    public static void PlayFxAtEndOfBar(FX type)
    {
        var clip = m_instance.m_lib[(int)type].Random;

        var headPos = m_instance.m_sourceBGMQ.timeSamples;

        var playPos = (headPos - (headPos % (m_instance.m_samplesPerBeat * 4))) + (m_instance.m_samplesPerBeat * 4);

        var delay = (float)(playPos - headPos) / m_instance.m_samplesPerSecond;

        m_instance.m_sourceFX.clip = clip;
        m_instance.m_sourceFX.PlayDelayed(delay);
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

    public static TimingInfo GetTimingInfo()
    {
        var x = new TimingInfo()
        {
            HeadPos = m_instance.m_sourceBGMQ.timeSamples,
            SamplesPerBeat = m_instance.m_samplesPerBeat
        };

        return x;
    }

    public static float GetFXVolume()
    {
        return m_instance.m_sourceFX.volume;
    }
}

public struct TimingInfo
{
    public int HeadPos;
    public int SamplesPerBeat;
}

// Waits til the end of the backgroudn musics 4-beat bar.
public class WaitForBeats : CustomYieldInstruction
{
    int playPos = -1;
    float beats;

    public override bool keepWaiting
    {
        get
        {
            var info = Audio.GetTimingInfo();
            if (playPos == -1) playPos = ((info.HeadPos - info.HeadPos % (info.SamplesPerBeat * 4))) + (int)(info.SamplesPerBeat * beats);

            return info.HeadPos < playPos;
        }
    }

    public WaitForBeats(float beats)
    {
        this.beats = beats;
    }
}