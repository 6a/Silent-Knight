using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReviveManager : MonoBehaviour
{
    [SerializeField] GameObject m_mainScreen, m_confirmReload, m_confirmReincarnate;

    public void Reincarnate()
    {
        GameManager.Reincarnate();
    }
    public void ReloadLevel()
    {
        GameManager.ReloadLevel();
    }

    public void OnPressReincarnate()
    {
        m_mainScreen.SetActive(false);
        m_confirmReload.SetActive(false);
        m_confirmReincarnate.SetActive(true);
    }

    public void OnPressReloadLevel()
    {
        m_mainScreen.SetActive(false);
        m_confirmReload.SetActive(true);
        m_confirmReincarnate.SetActive(false);
    }

    public void OnCancelConfirmation()
    {
        m_mainScreen.SetActive(true);
        m_confirmReload.SetActive(false);
        m_confirmReincarnate.SetActive(false);
    }

}
