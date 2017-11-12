using System.Collections.Generic;
using UnityEngine;

public class PlatformBounds
{
    public Vector2 BottomLeft { get; set; }
    public Vector2 TopRight { get; set; }

    public bool PlayerIsWithinBounds { get; set; }

    public PlatformBounds(Vector2 bottomLeft, Vector2 topRight)
    {
        BottomLeft = bottomLeft;
        TopRight = topRight;

        PlayerIsWithinBounds = false;
    }

    public bool IsInBounds(Vector2 pos)
    {
        if (pos.x < BottomLeft.x || pos.x > TopRight.x || pos.y < BottomLeft.y || pos.y > TopRight.y)
        {
            return false;
        }

        return true;
    }

    public Vector2 GetRandomLocationOnPlatform(int padding)
    {
        int xRand = Random.Range(0 + padding, (int)TopRight.x - (int)BottomLeft.x - padding);
        int yRand = Random.Range(0 + padding, (int)TopRight.y - (int)BottomLeft.y - padding);

        return new Vector2(BottomLeft.x + xRand, BottomLeft.y + yRand);
    }
}

public class Platforms : MonoBehaviour
{
    public static int PlayerPlatform { get; private set; }

    public List<PlatformBounds> m_platformBounds { get; private set; }

    static Platforms instance;

    JKnightControl m_player;

    void Awake()
    {
        instance = this;

        PlayerPlatform = -1;
    }

    void LateUpdate()
    {
        if (!m_player) return;
        PlayerPlatform = GetPlatformId(m_player.transform.position);
        m_player.CurrentPlatformIndex = PlayerPlatform;
        if (PlayerPlatform != -1)
        {
            m_platformBounds[PlayerPlatform].PlayerIsWithinBounds = true;
            m_player.OnEnterPlatform();
            AI.ActivateUnits(PlayerPlatform);
        }
    }

    public static void RegisterPlayer(JKnightControl player)
    {
        instance.m_player = player;
    }

    public static void UnregisterPlayer()
    {
        instance.m_player = null;
    }

    public static List<PlatformBounds> GetPlatformData()
    {
        return instance.m_platformBounds;
    }

    public static int GetPlatformId(Vector3 pos)
    {
        int id = -1;

        for (int i = 0; i < instance.m_platformBounds.Count; i++)
        {
            if (instance.m_platformBounds[i].IsInBounds(new Vector2(pos.x, pos.z)))
            {
                id = i;
            }
            else
            {
                instance.m_platformBounds[i].PlayerIsWithinBounds = false;
            }
        }

        return id;
    }

    public static void Identify(List<PlatformBounds> platforms)
    {
        instance.m_platformBounds = new List<PlatformBounds>(platforms);
    }

    private void OnDrawGizmos()
    {
        if (m_platformBounds == null) return;
        foreach (var platform in m_platformBounds)
        {
            if (platform.PlayerIsWithinBounds) Gizmos.color = Color.green;
            else Gizmos.color = Color.magenta;

            Gizmos.DrawLine(new Vector3(platform.BottomLeft.x, 1.5f, platform.BottomLeft.y), new Vector3(platform.BottomLeft.x, 1.5f, platform.TopRight.y));
            Gizmos.DrawLine(new Vector3(platform.BottomLeft.x, 1.5f, platform.TopRight.y), new Vector3(platform.TopRight.x, 1.5f, platform.TopRight.y));
            Gizmos.DrawLine(new Vector3(platform.TopRight.x, 1.5f, platform.TopRight.y), new Vector3(platform.TopRight.x, 1.5f, platform.BottomLeft.y));
            Gizmos.DrawLine(new Vector3(platform.BottomLeft.x, 1.5f, platform.BottomLeft.y), new Vector3(platform.TopRight.x, 1.5f, platform.BottomLeft.y));
        }
    }
}
