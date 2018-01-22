using UnityEngine;

/// <summary>
/// Handles Revival behaviour.
/// </summary>
public class ReviveManager : MonoBehaviour
{
    // References to UI objects for reset behaviour chosen by the player.
    [SerializeField] GameObject m_mainScreen, m_confirmReload, m_confirmReincarnate;

    // References to UI objects for reset behaviour shown after the player dies.
    [SerializeField] GameObject m_mainOnDeath, m_confirmReloadOnDeath, m_confirmReincarnateOnDeath;

    /// <summary>
    /// Perform reincarnation behaviours.
    /// </summary>
    public void Reincarnate()
    {
        GameManager.Reincarnate();
    }

    /// <summary>
    /// Perform level reload behaviours.
    /// </summary>
    public void ReloadLevel()
    {
        GameManager.ReloadLevel();
    }

    /// <summary>
    /// Button - Reincarnation.
    /// </summary>
    public void OnPressReincarnate()
    {
        m_mainScreen.SetActive(false);
        m_confirmReload.SetActive(false);
        m_confirmReincarnate.SetActive(true);
    }

    /// <summary>
    /// Button - Level reload.
    /// </summary>
    public void OnPressReloadLevel()
    {
        m_mainScreen.SetActive(false);
        m_confirmReload.SetActive(true);
        m_confirmReincarnate.SetActive(false);
    }

    /// <summary>
    /// Button - Cancelling of confirmation screen.
    /// </summary>
    public void OnCancelConfirmation()
    {
        m_mainScreen.SetActive(true);
        m_confirmReload.SetActive(false);
        m_confirmReincarnate.SetActive(false);
    }

    /// <summary>
    /// Button - Reincarnation, after player death.
    /// </summary>
    public void OnDeathPressReincarnate()
    {
        m_mainOnDeath.SetActive(false);
        m_confirmReloadOnDeath.SetActive(false);
        m_confirmReincarnateOnDeath.SetActive(true);
    }

    /// <summary>
    /// Button - level reload, after player death.
    /// </summary>
    public void OnDeathPressReloadLevel()
    {
        m_mainOnDeath.SetActive(false);
        m_confirmReloadOnDeath.SetActive(true);
        m_confirmReincarnateOnDeath.SetActive(false);
    }

    /// <summary>
    /// Button - Cancelling of confirmation screen after player death.
    /// </summary>
    public void OnDeathCancelConfirmation()
    {
        m_mainOnDeath.SetActive(true);
        m_confirmReloadOnDeath.SetActive(false);
        m_confirmReincarnateOnDeath.SetActive(false);
    }

}
