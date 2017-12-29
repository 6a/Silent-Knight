using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [SerializeField] GameObject m_pauseScreen, m_gameScreen;

    public static bool Paused;

	void Start ()
    {
		
	}
	
	void Update ()
    {
		
	}

    public void OnStartPause()
    {
        m_gameScreen.SetActive(false);
        m_pauseScreen.SetActive(true);
        Time.timeScale = 0;
        Camera.main.gameObject.GetComponent<UnityStandardAssets.ImageEffects.Blur>().enabled = true;
        Paused = true;
    }

    public void OnEndPause()
    {
        m_gameScreen.SetActive(true);
        m_pauseScreen.SetActive(false);
        Time.timeScale = 1;
        Camera.main.gameObject.GetComponent<UnityStandardAssets.ImageEffects.Blur>().enabled = false;
        Paused = false;
    }
}
