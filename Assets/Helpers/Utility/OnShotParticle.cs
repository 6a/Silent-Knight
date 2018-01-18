using System.Collections;
using UnityEngine;

/// <summary>
/// Component to add to particle gameobjects, allowing for timed auto deletion. Also sets attached Audiosource's volume to match the global FX volume
/// </summary>
public class OnShotParticle : MonoBehaviour
{
    [SerializeField] float m_lifetime;
    AudioSource m_attachedAudioSource;

	void Awake ()
    {
        m_attachedAudioSource = GetComponentInChildren<AudioSource>();
        m_attachedAudioSource.volume = AudioManager.GetSFXVolume();

        StartCoroutine(DelayDestroy());
	}

    IEnumerator DelayDestroy()
    {
        yield return new WaitForSeconds(m_lifetime);

        Destroy(gameObject);
    }
}
