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
    static Platforms instance;
    List<PlatformBounds> m_platformBounds;
    JKnightControl m_player;
    public int CurrentPlatformID { get; private set; }

    void Awake()
    {
        instance = this;
    }

    void LateUpdate()
    {
        if (m_player == null) return;

        CurrentPlatformID = -1;

        for (int i = 0; i < m_platformBounds.Count; i++)
        {
            if (m_platformBounds[i].IsInBounds(m_player.transform.position))
            {
                m_platformBounds[i].PlayerIsWithinBounds = true;
                CurrentPlatformID = i;
            }
            else
            {
                m_platformBounds[i].PlayerIsWithinBounds = false;
            }
        }
    }

    public static void Identify(List<PlatformBounds> platforms)
    {
        instance.m_platformBounds = new List<PlatformBounds>(platforms);
    }

    public static void RegisterPlayer(JKnightControl player)
    {
        instance.m_player = player;
    }

    public static void UnregisterPlayer()
    {
        instance.m_player = null;
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
