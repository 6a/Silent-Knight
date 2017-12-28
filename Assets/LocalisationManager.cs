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
        [SerializeField] List<TextField> m_textFields;
        [SerializeField] LANGUAGE m_currentLanguage;

        static LocalisationManager m_instance;

        void Awake()
        {
            m_instance = this;
        }

        public void SetLanguage(LANGUAGE lang)
        {
            foreach (var tf in m_textFields)
            {
                if (tf.isActiveAndEnabled) tf.SetLanguage(lang);
            }
        }

        public static LANGUAGE GetCurrentLanguage()
        {
            return m_instance.m_currentLanguage;
        }
    }
}

