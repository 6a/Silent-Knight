using UnityEngine;
using UnityEngine.UI;

namespace Localisation
{
    public class DynamicTextField : MonoBehaviour
    {
        const string FIELD1 = "<f1>";
        const string FIELD2 = "<f2>";
        const string SPACE = "<s>";

        [Tooltip("0 = EN ver, 1 = JP ver")] public string[] m_data = new string[2];
        [SerializeField] Text m_textField;
        [SerializeField] StatAdjuster m_button;

        [Tooltip("0 = EN ver, 1 = JP ver")] public string[] m_altData = new string[2];

        public Enums.LANGUAGE m_currentlanguage;

        public string Value1 { get; set; }
        public string Value2 { get; set; }

        bool m_ready = false;
        bool m_showingAltData = false;

        void Start()
        {
            LocalisationManager.DynamicFields.Add(this);

            UpdateLanguage();
            m_ready = true;
        }

        void OnEnable()
        {
            if (!m_ready) return;
            UpdateLanguage();
        }

        public void UpdateLanguage()
        {
            m_currentlanguage = LocalisationManager.GetCurrentLanguage();
            SetLanguage(m_currentlanguage);
        }

        public void SetLanguage(Enums.LANGUAGE lang)
        {
            string s = string.Empty;
            if (m_showingAltData)
            {
                s = m_altData[(int)lang];
            }
            else
            {
                s = m_data[(int)lang].Replace(SPACE, "\n").Replace(FIELD1, Value1).Replace(FIELD2, Value2);
            }

            m_textField.text = s;
            m_currentlanguage = lang;
        }

        public void SetButtonState(Enums.BONUS_STATE state, bool nopoints = false)
        {
            if (nopoints)
            {
                m_showingAltData = state == Enums.BONUS_STATE.AT_MAXIMUM;
            }

            m_button.SetState(state);
            UpdateLanguage();
        }

        public void HideButton(bool down)
        {
            m_button.HideButton(down);
        }
    }
}
