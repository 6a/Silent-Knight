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

    public bool IsInBounds(Vector3 playerPos)
    {
        if (playerPos.x >= BottomLeft.x && playerPos.x <= TopRight.x && playerPos.z >= BottomLeft.y && playerPos.z <= TopRight.y)
        {
            return true;
        }

        return false;
    }
}

public class Platforms : MonoBehaviour
{
    public int PlayerPlatform { get; private set; }

    static Platforms instance;
    List<PlatformBounds> m_platformBounds;
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

    public static int GetPlatformId(Vector3 pos)
    {
        int id = -1;

        for (int i = 0; i < instance.m_platformBounds.Count; i++)
        {
            if (instance.m_platformBounds[i].IsInBounds(pos))
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
