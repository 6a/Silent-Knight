using UnityEngine;
using UnityEngine.UI;
using SilentKnight.Utility;
using SilentKnight.UI.Bonus;

namespace SilentKnight.Localisation
{
    /// <summary>
    /// Dyanmic text class that can handle two different text versions. Makes use of simple markup to add modifiable fields.
    /// </summary>
    public class DynamicTextField : MonoBehaviour
    {
        // Const values for markup - these tags are identified and then replaced with the relevent text.
        const string FIELD1 = "<f1>";
        const string FIELD2 = "<f2>";
        const string SPACE = "<s>";

        // The two versions of text to be used.
        [Tooltip("0 = EN ver, 1 = JP ver")] public string[] m_data = new string[2];

        // Text UI object that the text will be displayed in.
        [SerializeField] Text m_textField;

        // The button that is attached to this text field's bonus unit.
        [SerializeField] StatAdjuster m_button;

        // Alternative text data, optional.
        [Tooltip("0 = EN ver, 1 = JP ver")] public string[] m_altData = new string[2];

        // Current language
        Enums.LANGUAGE m_currentlanguage;

        // The values that will be written over the markup tags.
        public string Value1 { get; set; }
        public string Value2 { get; set; }

        // State data.
        bool m_ready = false;
        bool m_showingAltData = false;

        void Start()
        {
            // Add a reference to this dynamic text field to the localisation manager.
            LocalisationManager.DynamicFields.Add(this);

            UpdateLanguage();
            m_ready = true;
        }

        // OnEnable is used to prevent some lag on startup - This textfield does not need to be updated until it is visible to the player.
        void OnEnable()
        {
            if (!m_ready) return;
            UpdateLanguage();
        }

        /// <summary>
        /// Updates the current language setting for this textfield and updates the text display.
        /// </summary>
        public void UpdateLanguage()
        {
            m_currentlanguage = LocalisationManager.GetCurrentLanguage();
            UpdateTextDisplay(m_currentlanguage);
        }

        /// <summary>
        /// Updates the textfield.
        /// </summary>
        void UpdateTextDisplay(Enums.LANGUAGE lang)
        {
            string s = string.Empty;
            if (m_showingAltData)
            {
                s = m_altData[(int)lang];
            }
            else
            {
                // String magic overwrites the markup patterns found, with the appropriate data.

                s = m_data[(int)lang].Replace(SPACE, "\n").Replace(FIELD1, Value1).Replace(FIELD2, Value2);
            }

            m_textField.text = s;
        }

        /// <summary>
        /// Sets the state for the associated button.
        /// </summary>
        public void SetButtonState(Enums.BONUS_STATE state, bool nopoints = false)
        {
            if (nopoints)
            {
                m_showingAltData = state == Enums.BONUS_STATE.AT_MAXIMUM;
            }

            m_button.SetState(state);
            UpdateLanguage();
        }

        /// <summary>
        /// Hides the associated button.
        /// </summary>
        public void HideButton(Enums.BUTTON_TYPE t)
        {
            m_button.HideButton(t);
        }
    }
}
