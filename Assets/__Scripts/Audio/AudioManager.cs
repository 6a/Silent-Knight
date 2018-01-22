using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using SilentKnight.Utility;

namespace SilentKnight.Audio
{
    /// <summary>
    /// Container and wrapper for audio clip reference.
    /// </summary>
    public struct AudioSet
    {
        // Location of Audio SFX files in the resources folder.
        const string ROOT_DIR = "Audio/SFX/";

        // The sub-folder + file name (without index).
        string m_file;

        // The number of files that are known to be available for this particular SFX type.
        public int Count { get; private set; }

        // The type of SFX represented by this container.
        public Enums.SFX_TYPE Type { get; private set; }

        /// <summary>
        /// Returns a random AudioClip from the available range.
        /// </summary>
        public AudioClip Random
        {
            get
            {
                if (Count == 1) return Resources.Load(ROOT_DIR + m_file + 1) as AudioClip;
                var ran = UnityEngine.Random.Range(1, Count + 1);
                AudioClip c = Resources.Load(ROOT_DIR + m_file + ran) as AudioClip;
                return c;
            }
        }

        /// <summary>
        /// Note: Only include folder structure past "Resources/Audio/SFX/".
        /// </summary>
        public AudioSet(Enums.SFX_TYPE clipType, int numberOfClips, string filePath)
        {
            Type = clipType;
            Count = numberOfClips;
            m_file = filePath;
        }
    }

    /// <summary>
    /// Handles all audio related functionality
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        // References to the AudioSources that are in use.
        [SerializeField] AudioSource m_sourceBGMQuiet, m_sourceBGMLoud, m_sourceSFX;

        // Reference to the Unity mixer.
        [SerializeField] AudioMixer m_masterMixer;

        // How long 1 bar of this song is, in seconds -> [ BMP / 60 ].
        [Tooltip("The (fixed) length of 1 bar for the background music, in seconds")] [SerializeField] float m_barTime;

        // List of all audio libraries.
        List<AudioSet> m_lib;

        // Reference to current audio blending coroutine.
        Coroutine m_blendRoutine;

        // Variables representing information about the loaded BGM clips.
        int m_samplesPerSecond;
        int m_samplesPerBeat;

        static AudioManager m_instance;

        void Awake()
        {
            // Set this gameobject up as a singleton to ensure that it to persists between scene loads and only exists as one instance.
            if (m_instance)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);

            m_lib = new List<AudioSet>();

            // Load sound library references to memory from Resources folder.
            AudioSet aSet = new AudioSet(Enums.SFX_TYPE.SWORD_IMPACT, 5, "player_sword/hit");
            m_lib.Add(aSet);

            aSet = new AudioSet(Enums.SFX_TYPE.FOOTSTEP, 2, "steps/step");
            m_lib.Add(aSet);

            aSet = new AudioSet(Enums.SFX_TYPE.ENEMY_ATTACK_IMPACT, 5, "enemy_attack_hit/HIT");
            m_lib.Add(aSet);

            aSet = new AudioSet(Enums.SFX_TYPE.KICK, 1, "kick/kick");
            m_lib.Add(aSet);

            aSet = new AudioSet(Enums.SFX_TYPE.SHIELD_SLAM, 1, "shield_slam/slam");
            m_lib.Add(aSet);

            aSet = new AudioSet(Enums.SFX_TYPE.SPELL_REFLECT, 3, "reflect/reflect");
            m_lib.Add(aSet);

            aSet = new AudioSet(Enums.SFX_TYPE.BIG_IMPACT, 1, "impact/impact");
            m_lib.Add(aSet);

            m_instance = this;
        }

        void Start()
        {
            // Set the various audio channels volumes after reading the values stored in persistent data.
            SetVolume(Enums.AUDIO_CHANNEL.BGM, PersistentData.LoadFloat(PersistentData.KEY_FLOAT.VOL_BGM));
            SetVolume(Enums.AUDIO_CHANNEL.SFX, PersistentData.LoadFloat(PersistentData.KEY_FLOAT.VOL_FX));
            SetVolume(Enums.AUDIO_CHANNEL.MASTER, PersistentData.LoadFloat(PersistentData.KEY_FLOAT.VOL_MASTER));

            // Calculate information about the loaded audio clips.
            m_samplesPerSecond = m_sourceBGMQuiet.clip.frequency;
            m_samplesPerBeat = (int)((m_barTime * m_samplesPerSecond) / 4);

            // Fade music in.
            CrossFadeBGM(Enums.BGM_VARIATION.QUIET, 2);
        }

        void Update()
        {
            KeepBGMTracksSynced();
        }

        /// <summary>
        /// If the (2) layers of the BGM are out of sync, re-sync them.
        /// </summary>
        void KeepBGMTracksSynced()
        {
            if (m_sourceBGMLoud.timeSamples != m_sourceBGMQuiet.timeSamples)
            {
                m_sourceBGMLoud.timeSamples = m_sourceBGMQuiet.timeSamples;
            }
        }

        /// <summary>
        /// Initialise an audio crossfade for the background music, with a particular transition duration (in beats).
        /// </summary>
        public static void CrossFadeBGM(Enums.BGM_VARIATION crossFadeTo, int beats = 2)
        {
            if (m_instance.m_blendRoutine != null) m_instance.StopCoroutine(m_instance.m_blendRoutine);

            m_instance.m_blendRoutine = m_instance.StartCoroutine(m_instance.CrossFadeBGMAsync(crossFadeTo, beats));
        }

        /// <summary>
        /// Fades between the Low and High volume audio channels.
        /// </summary>
        IEnumerator CrossFadeBGMAsync(Enums.BGM_VARIATION to, int beats)
        {
            // Const value for the wait between each adjustment of the volume during a fade.
            const float STEP = 0.02f;

            // Calculate the current playhead position.
            var headPos = m_sourceBGMQuiet.timeSamples;

            // Calculate the playhead position at which the audio transition should start.
            var playPos = (headPos % m_samplesPerBeat) + m_samplesPerBeat;

            // Wait until the desired playhead position is reached.
            while (m_sourceBGMQuiet.timeSamples < playPos)
            {
                yield return null;
            }

            // Calculate how many times to iterate through the loop, based on BGM timing data and chosen STEP.
            var iterations = Mathf.CeilToInt((beats * m_samplesPerBeat / (float)m_samplesPerSecond) / STEP);

            // Calculate how much to adjust the volume by for each iteration.
            var increments = (1f / iterations);

            // Boolean for detecting early exit conditions
            bool esc = false;

            // While there are still iterations to execute OR an exit condition is found, adjust the appropriate
            // AudioSource volume levels.
            while (iterations > 0 && !esc)
            {
                if (to == Enums.BGM_VARIATION.LOUD)
                {
                    m_sourceBGMQuiet.volume -= increments;
                    m_sourceBGMLoud.volume += increments;
                }
                else
                {
                    m_sourceBGMQuiet.volume += increments;
                    m_sourceBGMLoud.volume -= increments;
                }

                // If someone the volume of the quiet version's AudioSource reaches the limit, exit early.
                if (m_sourceBGMQuiet.volume == 0 || m_sourceBGMQuiet.volume == 1) esc = true;

                iterations--;
                yield return new WaitForSecondsRealtime(STEP);
            }

            // Once the transition is thought to be complete, set the AudioSource values to integers to 
            // remove any potential floating point inaccuracies that occured.
            if (to == Enums.BGM_VARIATION.LOUD)
            {
                m_sourceBGMLoud.volume = 1;
                m_sourceBGMQuiet.volume = 0;
            }
            else
            {
                m_sourceBGMQuiet.volume = 1;
                m_sourceBGMLoud.volume = 0;
            }
        }

        /// <summary>
        /// Set the volume for a particular channel. [Range is 0 - 1 (0% > 100%)].
        /// </summary>
        public static void SetVolume(Enums.AUDIO_CHANNEL type, float vol)
        {
            vol = Mathf.Clamp01(vol);

            switch (type)
            {
                case Enums.AUDIO_CHANNEL.MASTER:
                    var mvol = m_instance.LinearToDecibel(vol);
                    m_instance.m_masterMixer.SetFloat("MasterVolume", mvol);
                    break;
                case Enums.AUDIO_CHANNEL.BGM:
                    var bvol = m_instance.LinearToDecibel(vol);
                    m_instance.m_masterMixer.SetFloat("BGMVolume", bvol);
                    break;
                case Enums.AUDIO_CHANNEL.SFX:
                    m_instance.m_sourceSFX.volume = vol;
                    break;
            }
        }

        /// <summary>
        /// Play an SFX clip. Clip is chosen randomly if there is more than 1 within that particular library.
        /// </summary>
        public static void PlayFX(Enums.SFX_TYPE type, Vector3? position = null)
        {
            // Get an appropriate AudioClip
            var clip = m_instance.m_lib[(int)type].Random;

            // If a position is passed in, play the clip at that position. Otherwise just play it on the SFX Audiosource.
            if (position.HasValue)
            {
                AudioSource.PlayClipAtPoint(clip, position.Value, m_instance.m_sourceSFX.volume);
            }
            else
            {
                m_instance.m_sourceSFX.clip = clip;
                m_instance.m_sourceSFX.Play();
            }
        }

        /// <summary>
        /// Returns the current SFX volume as a float between 0 and 1.
        /// </summary>
        public static float GetSFXVolume()
        {
            return m_instance.m_sourceSFX.volume;
        }

        /// <summary>
        /// Returns a [long] PWN pattern based on the desired vibration intensity and duration. 
        /// </summary>
        /// SOURCE: Liandur @ Unity Answers - https://answers.unity.com/answers/1107168/view.html
        float LinearToDecibel(float linear)
        {
            float dB;

            if (linear != 0)
                dB = 20.0f * Mathf.Log10(linear);
            else
                dB = -144.0f;

            return dB;
        }
    }
}