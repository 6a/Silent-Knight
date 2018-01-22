using UnityEngine;

namespace SilentKnight.Utility
{
    /// <summary>
    /// Wrapper for Unity PlayerPrefs. Save and Load data using the member enum keys.
    /// </summary>
    public static class PersistentData
    {
        // various magic strings that are used to create/identify storage identifiers for various datatypes
        const string KEY_FIRST_RUN = "9qwh10h812d97h120ehj8k";
        const string BOOL_PREFIX = "_BOOLEAN_TYPE_";
        const string BOOL_TRUE = "1";
        const string BOOL_FALSE = "0";

        /// <summary>
        /// Enums representing keys for integer based values.
        /// </summary>
        public enum KEY_INT
        {
            XP, LEVEL, BONUS_CRIT_CHANCE, BONUS_ATTACK_DAMAGE, BONUS_ULT_DUR,
            BONUS_KICK_CD, BONUS_SPIN_CD, BONUS_BASH_CD, BONUS_REFLECT_CD,
            BONUS_ULT_CD, BONUS_HEALTH, BONUS_DODGE_CHANCE, CURRENT_CREDITS,
            SPENT_CREDITS, LEVEL_GFX, LANGUAGE
        }

        /// <summary>
        /// Enums representing keys for float based values.
        /// </summary>
        public enum KEY_FLOAT { VOL_MASTER, VOL_BGM, VOL_FX }

        /// <summary>
        /// Enums representing keys for bolean based values.
        /// </summary>
        public enum KEY_BOOL { HAPTIC_FEEDBACK }

        /// <summary>
        /// Saves an integer value to PlayerPrefs.
        /// </summary>
        public static void SaveInt(KEY_INT key, int value)
        {
            PlayerPrefs.SetInt(key.ToString(), value);
        }

        /// <summary>
        /// Returns an integer value from PlayerPrefs.
        /// </summary>
        public static int LoadInt(KEY_INT key)
        {
            return PlayerPrefs.GetInt(key.ToString());
        }

        /// <summary>
        /// Saves a float value to PlayerPrefs.
        /// </summary>
        public static void SaveFloat(KEY_FLOAT key, float value)
        {
            PlayerPrefs.SetFloat(key.ToString(), value);
        }

        /// <summary>
        /// Returns a integer float from PlayerPrefs.
        /// </summary>
        public static float LoadFloat(KEY_FLOAT key)
        {
            return PlayerPrefs.GetFloat(key.ToString());
        }

        // Note: booleans are stored internally as a "0" or "1" string value.
        // They are identified by prependending the appropriate KEY_BOOL with BOOL_PREFIX

        /// <summary>
        /// Saves a boolean value to PlayerPrefs.
        /// </summary>
        public static void SaveBool(KEY_BOOL key, bool value)
        {
            var v = (value) ? BOOL_TRUE : BOOL_FALSE;

            PlayerPrefs.SetString(BOOL_PREFIX + (key.ToString()), v);
        }

        /// <summary>
        /// Returns a boolean from PlayerPrefs.
        /// </summary>
        public static bool LoadBool(KEY_BOOL key)
        {
            var value = PlayerPrefs.GetString(BOOL_PREFIX + (key.ToString()));

            if (value == BOOL_TRUE) return true;
            else if (value == BOOL_FALSE) return false;
            else throw new System.InvalidOperationException("Tried to load a boolean but read a strange value: " + value);
        }

        /// <summary>
        /// Returns true if this is the first time that the game has been run on this device, or since PlayerPrefs cache was cleared/deleted
        /// </summary>
        public static bool FirstRun()
        {
            return PlayerPrefs.GetInt(KEY_FIRST_RUN) == 0;
        }

        /// <summary>
        /// Updates PlayerPrefs to confirm that the first run has occured.
        /// </summary>
        public static void ConfirmFirstRun()
        {
            PlayerPrefs.SetInt(KEY_FIRST_RUN, 1);
        }
    }
}