using System.Collections;
using UnityEngine;
using SilentKnight.Audio;

namespace SilentKnight.Utility
{
    /// <summary>
    /// Component to add to particle gameobjects, allowing for timed auto deletion. Also sets attached Audiosource's volume to match the global FX volume
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class OnShotParticle : MonoBehaviour
    {
        [SerializeField] float m_lifetime;
        AudioSource m_attachedAudioSource;

        void Awake()
        {
            // Locate the attached AudioSource and set it's volume to match the global SFX volume.
            m_attachedAudioSource = GetComponentInChildren<AudioSource>();
            m_attachedAudioSource.volume = AudioManager.GetSFXVolume();

            // Start the delayed destroy coroutine.
            StartCoroutine(DelayDestroy());
        }

        /// <summary>
        /// Destroys this GameObject after the set delay duration has passed.
        /// </summary>
        IEnumerator DelayDestroy()
        {
            yield return new WaitForSeconds(m_lifetime);

            Destroy(gameObject);
        }
    }
}