using System.Collections;
using UnityEngine;

public class OnShotParticle : MonoBehaviour
{
    [SerializeField] float m_t;
    [SerializeField] AudioSource m_as;

	void Awake ()
    {
        m_as.volume = Audio.GetFXVolume();

        StartCoroutine(DelayDestroy());
	}

    IEnumerator DelayDestroy()
    {
        yield return new WaitForSeconds(m_t);

        Destroy(gameObject);
    }
}
