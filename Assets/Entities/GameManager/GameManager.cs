using System.Collections;
using UnityEngine;


public delegate void LevelLoadTrigger();

public class GameManager : MonoBehaviour
{
    public static GAMESTATE GameState { get; set; }

    public static event LevelLoadTrigger OnStartRun;

    DungeonGenerator m_generator;

    void Awake()
    {
        m_generator = FindObjectOfType<DungeonGenerator>();
        OnStartRun += NextLevelSequence;
        GameState = GAMESTATE.START;
    }

    void Start ()
    {
        // Startup sequence
        GameState = GAMESTATE.GAMEPLAY;

        LoadNext();
	}

	void Update ()
    {
		
	}

    public void NextLevelSequence()
    {
        Debug.Log("GAME MANAGER: Next Level Trigger received");
    }

    void LoadNext()
    {
        m_generator.LoadNext();

        OnStartRun();
    }

    public static void TriggerLevelLoad ()
    {
        if (GameState == GAMESTATE.GAMEPLAY && OnStartRun != null)
        {
            GameState = GAMESTATE.LEVELTRANSITION;
        }
    }
}
