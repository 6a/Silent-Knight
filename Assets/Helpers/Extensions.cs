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
}
