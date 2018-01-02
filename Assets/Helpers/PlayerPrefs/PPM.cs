using UnityEngine;

public static class PPM
{
    public enum KEY_INT { XP }
    public enum KEY_FLOAT { NULL }
    public enum KEY_STR { NULL }

    public static void SaveInt(KEY_INT key, int value)
    {
        PlayerPrefs.SetInt(key.ToString(), value);
    }

    public static int LoadInt(KEY_INT key)
    {
        return PlayerPrefs.GetInt(key.ToString());
    }

    public static void SaveFloat(KEY_FLOAT key, float value)
    {
        PlayerPrefs.SetFloat(key.ToString(), value);
    }

    public static float LoadFloat(KEY_FLOAT key)
    {
        return PlayerPrefs.GetFloat(key.ToString());
    }

    public static void SaveString(KEY_STR key, string value)
    {
        PlayerPrefs.SetString(key.ToString(), value);
    }

    public static string LoadString(KEY_STR key)
    {
        return PlayerPrefs.GetString(key.ToString());
    }
}
