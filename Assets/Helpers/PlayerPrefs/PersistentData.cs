using UnityEngine;

public static class PersistentData
{
    public enum KEY_INT
    {
        XP, LEVEL, BONUS_CRIT_CHANCE, BONUS_ATTACK_DAMAGE, BONUS_ULT_DUR,
        BONUS_KICK_CD, BONUS_SPIN_CD, BONUS_BASH_CD, BONUS_DEFLECT_CD,
        BONUS_ULT_CD, BONUS_HEALTH, BONUS_DODGE_CHANCE, CURRENT_CREDITS,
        SPENT_CREDITS, LEVEL_GFX, LANGUAGE
    }

    public enum KEY_FLOAT
    {
        VOL_MASTER, VOL_BGM, VOL_FX
    }

    public enum KEY_STR
    {
        NULL
    }

    public enum KEY_BOOL
    {
        HAPTIC_FEEDBACK
    }

    const string KEY_FIRST_RUN = "9qwh10h812d97h120ehj8k";

    const string BOOL_PREFIX = "_BOOLEAN_TYPE_";
    const string BOOL_TRUE = "1";
    const string BOOL_FALSE = "0";

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

    public static void SaveBool(KEY_BOOL key, bool value)
    {
        var v = (value) ? BOOL_TRUE : BOOL_FALSE;

        PlayerPrefs.SetString(BOOL_PREFIX + (key.ToString()), v);
    }

    public static bool LoadBool(KEY_BOOL key)
    {
        var value = PlayerPrefs.GetString(BOOL_PREFIX + (key.ToString()));

        if (value == BOOL_TRUE) return true;
        else if (value == BOOL_FALSE) return false;
        else throw new System.InvalidOperationException("Tried to load a boolean but read a strange value: " + value);
    }

    public static bool FirstRun()
    {
        return PlayerPrefs.GetInt(KEY_FIRST_RUN) == 0;
    }

    public static void ConfirmFirstRun()
    {
        PlayerPrefs.SetInt(KEY_FIRST_RUN, 1);
    }
}
