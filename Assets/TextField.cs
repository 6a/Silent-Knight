using UnityEngine;
using UnityEngine.UI;


namespace Localisation
{
    public class TextField : MonoBehaviour
    {
        [Tooltip("0 = EN ver, 1 = JP ver")] public string[] m_data = new string[2];
        [SerializeField] Text m_textField;
        public LANGUAGE m_currentlanguage;

        bool m_ready = false;

        void Start()
        {
            UpdateLanguage();
            m_ready = true;
        }

        void OnEnable()
        {
            if (!m_ready) return;
            UpdateLanguage();
        }

        void UpdateLanguage()
        {
            m_currentlanguage = LocalisationManager.GetCurrentLanguage();
            SetLanguage(m_currentlanguage);
        }

        public void SetLanguage(LANGUAGE lang)
        {
            m_textField.text = m_data[(int)lang];
            m_currentlanguage = lang;
        }
    }
}
