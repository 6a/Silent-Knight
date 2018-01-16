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

    public List<PlatformBounds> PlatformBounds { get; private set; }

    static Platforms instance;

    PlayerPathFindingObject m_player;

    void Awake()
    {
        instance = this;

        PlayerPlatform = -1;
    }

    void LateUpdate()
    {
        if (!m_player) return;

        var p = GetPlatformId(m_player.transform.position);

        if (p != -1) PlayerPlatform = p;

        if (m_player.CurrentPlatformIndex != PlayerPlatform) m_player.OnEnterPlatform();

        m_player.CurrentPlatformIndex = PlayerPlatform;

        PlatformBounds[PlayerPlatform].PlayerIsWithinBounds = true;

        AI.ActivateUnits(PlayerPlatform);
    }

    public static void RegisterPlayer(PlayerPathFindingObject player)
    {
        instance.m_player = player;
    }

    public static void UnregisterPlayer()
    {
        instance.m_player = null;
    }

    public static List<PlatformBounds> GetPlatformData()
    {
        return instance.PlatformBounds;
    }

    public static int GetPlatformId(Vector3 pos)
    {
        int id = -1;

        for (int i = 0; i < instance.PlatformBounds.Count; i++)
        {
            if (instance.PlatformBounds[i].IsInBounds(new Vector2(pos.x, pos.z)))
            {
                id = i;
            }
            else
            {
                instance.PlatformBounds[i].PlayerIsWithinBounds = false;
            }
        }

        return id;
    }

    public static void Identify(List<PlatformBounds> platforms)
    {
        instance.PlatformBounds = new List<PlatformBounds>(platforms);
    }

    private void OnDrawGizmos()
    {
        if (PlatformBounds == null) return;
        foreach (var platform in PlatformBounds)
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
