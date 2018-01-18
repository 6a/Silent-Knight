using UnityEngine;

public class ReviveManager : MonoBehaviour
{
    [SerializeField] GameObject m_mainScreen, m_confirmReload, m_confirmReincarnate;

    [SerializeField] GameObject m_mainOnDeath, m_confirmReloadOnDeath, m_confirmReincarnateOnDeath;

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

    public void OnDeathPressReincarnate()
    {
        m_mainOnDeath.SetActive(false);
        m_confirmReloadOnDeath.SetActive(false);
        m_confirmReincarnateOnDeath.SetActive(true);
    }

    public void OnDeathPressReloadLevel()
    {
        m_mainOnDeath.SetActive(false);
        m_confirmReloadOnDeath.SetActive(true);
        m_confirmReincarnateOnDeath.SetActive(false);
    }

    public void OnDeathCancelConfirmation()
    {
        m_mainOnDeath.SetActive(true);
        m_confirmReloadOnDeath.SetActive(false);
        m_confirmReincarnateOnDeath.SetActive(false);
    }

}
