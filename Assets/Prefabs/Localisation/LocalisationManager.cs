using System.Collections.Generic;
using UnityEngine;

namespace Localisation
{
    /// <summary>
    /// Handles all EN/JP dual language text objects.
    /// </summary>
    public class LocalisationManager : MonoBehaviour
    {
        // List of all standard dual language text fields in the scene.
        public static List<TextField> TextFields { get; set; }

        // List of all dynamic dual language text fields in the scene.
        public static List<DynamicTextField> DynamicFields { get; set; }

        // The current language setting. This is overwritten on awake, and is just for display purposes. DO NOT EDIT IN EDITOR.
        [Tooltip("DO NOT MODIFY - DEBUGGING PURPOSES ONLY")][SerializeField] Enums.LANGUAGE m_currentLanguage;

        static LocalisationManager m_instance;

        void Awake()
        {
            m_instance = this;

            // Find all text objects within the scene.
            TextFields = new List<TextField>();
            DynamicFields = new List<DynamicTextField>();

            // Load the current language setting from persistent data.
            m_currentLanguage = (Enums.LANGUAGE)PersistentData.LoadInt(PersistentData.KEY_INT.LANGUAGE);
        }

        /// <summary>
        /// Update the current language. This also updates all known text objects.
        /// </summary>
        public static void SetLanguage(Enums.LANGUAGE lang)
        {
            m_instance.m_currentLanguage = lang;
            foreach (var tf in TextFields)
            {
                if (tf.isActiveAndEnabled) tf.SetLanguage(lang);
            }
        }

        /// <summary>
        /// Store the current language to persistent data.
        /// </summary>
        public static void SaveLanguage()
        {
            PersistentData.SaveInt(PersistentData.KEY_INT.LANGUAGE, (int)m_instance.m_currentLanguage);
        }

        /// <summary>
        /// Returns the current language setting.
        /// </summary>
        public static Enums.LANGUAGE GetCurrentLanguage()
        {
            return m_instance.m_currentLanguage;
        }

        /// <summary>
        /// Returns a string representation of the currency to be used for this particular language.
        /// </summary>
        /// <returns></returns>
        public static string GetCurrency()
        {
            switch (m_instance.m_currentLanguage)
            {
                case Enums.LANGUAGE.EN:     return "$";
                case Enums.LANGUAGE.JP:     return "￥";
                default:                    return "GALD";
            }
        }
    }
}

