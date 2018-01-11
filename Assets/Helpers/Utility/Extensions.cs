using System;

public static class Extension
{
    public static UnityEngine.Vector2 Snap(this UnityEngine.Vector2 v)
    {
        v = v.normalized;

        var Y = v.y;
        var X = v.x;

        if (UnityEngine.Mathf.Abs(Y) >= UnityEngine.Mathf.Abs(X))
        {
            if (Y > 0)
            {
                return UnityEngine.Vector2.up;
            }
            else
            {
                return UnityEngine.Vector2.down;
            }
        }
        else
        {
            if (X > 0)
            {
                return UnityEngine.Vector2.right;
            }
            else
            {
                return UnityEngine.Vector2.left;
            }
        }
    }

    public static UnityEngine.Vector2 Reduce(this UnityEngine.Vector3 v)
    {
        return new UnityEngine.Vector2(v.x, v.z);
    }

    public static string Format(this float f, string suffix)
    {
        const float BILLION = 1000000000;
        const float MILLION = 1000000;
        const float THOUSAND = 1000;

        if (f >= BILLION) return Math.Round(f / BILLION, 3).ToString() + "B";
        else if (f >= MILLION) return Math.Round(f / MILLION, 3).ToString() + "M";
        else if (f >= THOUSAND) return Math.Round(f / THOUSAND, 2).ToString() + "K";
        else return Math.Round(f, 1).ToString() + suffix;
    }

        public static void SetLayerRecursively(this UnityEngine.GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (UnityEngine.Transform child in obj.transform)
        {
            child.gameObject.SetLayerRecursively(layer);
        }
    }
}


