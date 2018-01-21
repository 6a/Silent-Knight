using Localisation;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    // References to various settings menu components.
    // The five different UI settings screens.
    [SerializeField] GameObject[] m_screens = new GameObject[5];

    // Button sprites for the settings selection panel.
    [SerializeField] Image[] m_buttonSprites = new Image[4];

    // Sprite representing a selected and non-selected button.
    [SerializeField] Sprite m_buttonSelected, m_buttonNotSelected;

    // Sliders found within the settings menu.
    [SerializeField] Slider m_masterVolumeSlider, m_bgmSlider, m_fxVolumeSlider, m_gfxQualitySlider;

    // Toggle for haptic feedback.
    [SerializeField] Toggle m_hapticFeedbackToggle;

    // Language select component of the settings menu.
    [SerializeField] LanguageSelect m_langSelect;

    // State of the settings menu.
    [SerializeField] Enums.SETTING_STATE m_state;

    // Storage for historical settings, and current settings. Used to track changes, for resetting and discarding,
    // allowing saves to persistent data to only be performed when comitting the changes made.
    Settings m_previousSettings, m_currentSettings;

    // Default values for the settings. Used for resetting to default.
    readonly Settings m_defaults = new Settings(0.8f, 0.8f, 0.8f, 2, true, Enums.LANGUAGE.JP);

    public static SettingsManager m_instance;

    void Start ()
    {
        // First run behaviour
        if (PersistentData.FirstRun())
        {
            PersistentData.ConfirmFirstRun();

            SetState((int)Enums.SETTING_STATE.GENERAL);

            m_currentSettings = m_previousSettings = m_defaults;

            RefreshDisplayedValues(m_currentSettings);

            OnSaveSettings();
        }

        m_instance = this;

        SetState((int)Enums.SETTING_STATE.GENERAL);

        // Load previous settings from persistent data.
        m_previousSettings = new Settings()
        {
            MasterVolume = PersistentData.LoadFloat(PersistentData.KEY_FLOAT.VOL_MASTER),
            BGMVolume = PersistentData.LoadFloat(PersistentData.KEY_FLOAT.VOL_BGM),
            SFXVolume = PersistentData.LoadFloat(PersistentData.KEY_FLOAT.VOL_FX),
            GFXLevel = PersistentData.LoadInt(PersistentData.KEY_INT.LEVEL_GFX),
            HapticFeedback = PersistentData.LoadBool(PersistentData.KEY_BOOL.HAPTIC_FEEDBACK),
            Language = LocalisationManager.GetCurrentLanguage(),
            Modified = false
        };

        m_currentSettings = m_previousSettings;

        // Update display.
        RefreshDisplayedValues(m_previousSettings);
    }

    /// <summary>
    /// Updates all settings to the values passed in.
    /// </summary>
    void RefreshDisplayedValues(Settings settings)
    {
        m_masterVolumeSlider.value = settings.MasterVolume;
        m_bgmSlider.value = settings.BGMVolume;
        m_fxVolumeSlider.value = settings.SFXVolume;
        m_gfxQualitySlider.value = settings.GFXLevel;
        m_hapticFeedbackToggle.isOn = settings.HapticFeedback;
        m_langSelect.ToggleDisplay(LocalisationManager.GetCurrentLanguage());
    }

    /// <summary>
    /// Sets the settings menu to a particular state, then updates the display.
    /// </summary>
    public void SetState(int state)
    {
        m_state = (Enums.SETTING_STATE)state;
        UpdateDisplay();
    }

    /// <summary>
    /// Updates the settings selection panel to show the currently selected settings section.
    /// </summary>
    /// <param name="i"></param>
    public void SetActiveButton(Image i)
    {
        foreach (var s in m_buttonSprites)
        {
            s.sprite = m_buttonNotSelected;
        }

        i.sprite = m_buttonSelected;
    }

    /// <summary>
    /// Reset everything to default, and update the display.
    /// </summary>
    public void OnResetToDefault()
    {
        m_currentSettings = m_defaults;
        m_currentSettings.Modified = true;

        UpdateGlobals();
    }

    /// <summary>
    /// Commits the various settings, with current settings.
    /// </summary>
    private void UpdateGlobals()
    {
        LocalisationManager.SetLanguage(m_currentSettings.Language);
        AudioManager.SetVolume(Enums.AUDIO_CHANNEL.MASTER, m_currentSettings.MasterVolume);
        AudioManager.SetVolume(Enums.AUDIO_CHANNEL.BGM, m_currentSettings.BGMVolume);
        AudioManager.SetVolume(Enums.AUDIO_CHANNEL.SFX, m_currentSettings.SFXVolume);
        GFXQuality.UpdateQuality((Enums.GFX_QUALITY)m_currentSettings.GFXLevel);

        RefreshDisplayedValues(m_currentSettings);
    }

    /// <summary>
    /// Button - handles back button behaviour.
    /// </summary>
    public void OnBack()
    {
        if (!m_currentSettings.Modified) return;
        m_currentSettings = m_instance.m_previousSettings;

        UpdateGlobals();
    }

    /// <summary>
    /// Save settings to persistent data (button usually).
    /// </summary>
    public void OnSaveSettings()
    {
        PersistentData.SaveFloat(PersistentData.KEY_FLOAT.VOL_MASTER, m_currentSettings.MasterVolume);
        PersistentData.SaveFloat(PersistentData.KEY_FLOAT.VOL_BGM, m_currentSettings.BGMVolume);
        PersistentData.SaveFloat(PersistentData.KEY_FLOAT.VOL_FX, m_currentSettings.SFXVolume);
        PersistentData.SaveInt(PersistentData.KEY_INT.LEVEL_GFX, m_currentSettings.GFXLevel);
        PersistentData.SaveBool(PersistentData.KEY_BOOL.HAPTIC_FEEDBACK, m_currentSettings.HapticFeedback);
        LocalisationManager.SaveLanguage();
        GFXQuality.UpdateQuality((Enums.GFX_QUALITY)m_currentSettings.GFXLevel);

        m_previousSettings = m_currentSettings;
        m_currentSettings.Modified = false;
        m_previousSettings.Modified = false;
    }

    /// <summary>
    /// Slider update - master volume.
    /// </summary>
    public void OnChangeMasterVolume(Slider slider)
    {
        var newVol = slider.value;
        if (newVol == m_currentSettings.MasterVolume) return;
        m_currentSettings.Modified = true;

        m_currentSettings.MasterVolume = newVol;

        AudioManager.SetVolume(Enums.AUDIO_CHANNEL.MASTER, newVol);
    }

    /// <summary>
    /// Slider update - BGM volume.
    /// </summary>
    public void OnChangeBGMVolume(Slider slider)
    {
        var newVol = slider.value;
        if (newVol == m_currentSettings.BGMVolume) return;
        m_currentSettings.Modified = true;

        m_currentSettings.BGMVolume = newVol;

        AudioManager.SetVolume(Enums.AUDIO_CHANNEL.BGM, newVol);
    }

    /// <summary>
    /// Slider update - SFX volume.
    /// </summary>
    public void OnChangeSFXVolume(Slider slider)
    {
        var newVol = slider.value;
        if (newVol == m_currentSettings.SFXVolume) return;
        m_currentSettings.Modified = true;

        m_currentSettings.SFXVolume = newVol;

        AudioManager.SetVolume(Enums.AUDIO_CHANNEL.SFX, newVol);
    }

    /// <summary>
    /// Slider update - GFX Settings.
    /// </summary>
    public void OnChangeGFXSettings(Slider slider)
    {
        var newVol = (int)slider.value;
        if (newVol == m_currentSettings.GFXLevel) return;
        m_currentSettings.Modified = true;

        m_currentSettings.GFXLevel = newVol;

        // TODO add code to actually change graphics quality
    }

    /// <summary>
    /// Toggle - Haptic feedback toggle.
    /// </summary>
    public void OnToggleHaptic(Toggle slider)
    {
        var on = slider.isOn;
        if (on == m_currentSettings.HapticFeedback) return;
        m_currentSettings.Modified = true;

        m_currentSettings.HapticFeedback = on;
    }

    /// <summary>
    /// Language button update behaviour.
    /// </summary>
    public void OnChangeLanguage(int lang)
    {
        if ((Enums.LANGUAGE)lang == m_currentSettings.Language) return;
        m_currentSettings.Language = (Enums.LANGUAGE)lang;
        m_currentSettings.Modified = true;
    }

    /// <summary>
    /// Call when the game is to be reset.
    /// </summary>
    public void OnResetGame()
    {
        GameManager.TotalReset();
    }

    /// <summary>
    /// Reset the settings panel display.
    /// </summary>
    void UpdateDisplay()
    {
        HideAll();

        m_screens[(int)m_state].SetActive(true);
    }

    /// <summary>
    /// Hide all settings panels.
    /// </summary>
    void HideAll()
    {
        foreach (var screen in m_screens)
        {
            screen.SetActive(false);
            
        }
    }

    /// <summary>
    /// Returns true if haptic feedback is turned on.
    /// </summary>
    public static bool Haptic()
    {
        return m_instance.m_previousSettings.HapticFeedback;
    }
}
