using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum LANGUAGE
{
    EN,
    JP
}

namespace Localisation
{
    public class LocalisationManager : MonoBehaviour
    {
        public static List<TextField> TextFields { get; set; }
        public static List<DynamicTextField> DynamicFields { get; set; }
        [SerializeField] LANGUAGE m_currentLanguage;

        static LocalisationManager m_instance;

        void Awake()
        {
            m_instance = this;
            TextFields = new List<TextField>();
            DynamicFields = new List<DynamicTextField>();
        }

        public void SetLanguage(LANGUAGE lang)
        {
            foreach (var tf in TextFields)
            {
                if (tf.isActiveAndEnabled) tf.SetLanguage(lang);
            }
        }

        public static LANGUAGE GetCurrentLanguage()
        {
            return m_instance.m_currentLanguage;
        }

        public static string GetCurrency()
        {
            switch (m_instance.m_currentLanguage)
            {
                case LANGUAGE.EN:   return "$";
                case LANGUAGE.JP:   return "￥";
                default:            return "GIL";
            }
        }
    }
}

