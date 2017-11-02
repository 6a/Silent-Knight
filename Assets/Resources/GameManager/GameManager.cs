using UnityEngine;


public delegate void LevelLoadTrigger();

public class GameManager : MonoBehaviour
{
    public static GAMESTATE GameState { get; set; }

    public static event LevelLoadTrigger OnLevelLoadTrigger;

    DungeonGenerator m_generator;

    void Awake()
    {
        m_generator = FindObjectOfType<DungeonGenerator>();
        OnLevelLoadTrigger += NextLevelSequence;
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
    }

    public static void TriggerLevelLoad ()
    {
        if (GameState == GAMESTATE.GAMEPLAY && OnLevelLoadTrigger != null)
        {
            GameState = GAMESTATE.LEVELTRANSITION;
            OnLevelLoadTrigger();
        }
    }
}
