using Localisation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguageSelect : MonoBehaviour
{
    [SerializeField] Image m_buttonEN, m_buttonJP;

    private void Start()
    {
        if (LocalisationManager.GetCurrentLanguage() == Enums.LANGUAGE.EN)
        {
            OnPressEN();
        }
        else
        {
            OnPressJP();
        }
    }

    public void OnPressJP()
    {
        m_buttonEN.enabled = false;
        m_buttonJP.enabled = true;
        LocalisationManager.SetLanguage(Enums.LANGUAGE.JP);
    }

    public void OnPressEN()
    {
        m_buttonEN.enabled = true;
        m_buttonJP.enabled = false;
        LocalisationManager.SetLanguage(Enums.LANGUAGE.EN);
    }

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
