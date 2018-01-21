using Localisation;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper class for language toggle buttons.
/// </summary>
public class LanguageSelect : MonoBehaviour
{
    // References to the two button images.
    [SerializeField] Image m_buttonEN, m_buttonJP;

    void Start()
    {
        // On startup, language is read, and display is updated.

        if (LocalisationManager.GetCurrentLanguage() == Enums.LANGUAGE.EN)
        {
            OnPressEN();
        }
        else
        {
            OnPressJP();
        }
    }

    /// <summary>
    /// Button - When the Japanese flag is clicked.
    /// </summary>
    public void OnPressJP()
    {
        m_buttonEN.enabled = false;
        m_buttonJP.enabled = true;
        LocalisationManager.SetLanguage(Enums.LANGUAGE.JP);
    }

    /// <summary>
    /// Button - When the English (American) flag is clicked.
    /// </summary>
    public void OnPressEN()
    {
        m_buttonEN.enabled = true;
        m_buttonJP.enabled = false;
        LocalisationManager.SetLanguage(Enums.LANGUAGE.EN);
    }

    /// <summary>
    /// Set the toggle to show the chosen language as the current selection.
    /// </summary>
    public void ToggleDisplay(Enums.LANGUAGE lang)
    {
        if (lang == Enums.LANGUAGE.EN)
        {
            m_buttonEN.enabled = true;
            m_buttonJP.enabled = false;
        }
        else
        {
            m_buttonEN.enabled = false;
            m_buttonJP.enabled = true;
        }
    }
}
