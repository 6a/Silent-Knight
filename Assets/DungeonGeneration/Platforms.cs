using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlatformBounds
{
    public Vector2 BottomLeft { get; set; }
    public Vector2 TopRight { get; set; }

    public PlatformBounds(Vector2 bottomLeft, Vector2 topRight)
    {
        BottomLeft = bottomLeft;
        TopRight = topRight;
    }
}

public class Platforms : MonoBehaviour
{
    static Platforms instance;
    List<PlatformBounds> m_platformBounds;

    void Awake()
    {
        instance = this;
    }

    public static void Identify(List<PlatformBounds> platforms)
    {
        instance.m_platformBounds = new List<PlatformBounds>(platforms);
    }

    int GetCurrentPlatformID()
    {
        return 0;
    }

    private void OnDrawGizmos()
    {
        if (m_platformBounds == null) return;
        foreach (var platform in m_platformBounds)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(new Vector3(platform.BottomLeft.x, 1.5f, platform.BottomLeft.y), new Vector3(platform.BottomLeft.x, 1.5f, platform.TopRight.y));
            Gizmos.DrawLine(new Vector3(platform.BottomLeft.x, 1.5f, platform.TopRight.y), new Vector3(platform.TopRight.x, 1.5f, platform.TopRight.y));
            Gizmos.DrawLine(new Vector3(platform.TopRight.x, 1.5f, platform.TopRight.y), new Vector3(platform.TopRight.x, 1.5f, platform.BottomLeft.y));
            Gizmos.DrawLine(new Vector3(platform.BottomLeft.x, 1.5f, platform.BottomLeft.y), new Vector3(platform.TopRight.x, 1.5f, platform.BottomLeft.y));
        }
    }
}
