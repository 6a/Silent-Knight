using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// TODO comments.
public class InstructionsManager : MonoBehaviour
{
    [SerializeField] GameObject[] m_langSelect;

    void Awake ()
    {
        //if (!PersistentData.FirstRun()) LoadGame();
	}
	
    public void LoadGame()
    {
        SceneManager.LoadSceneAsync(1);
    }
}
