using Localisation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

struct Settings
{
    public float MasterVolume, BGMVolume, FXVolume;
    public int GFXLevel;
    public bool HapticFeedback;
    public LANGUAGE Language;
    public bool Modified;

    public Settings(float masterVolume, float bGMVolume, float fXVolume, int gFXLevel, bool hapticFeedback, LANGUAGE language)
    {
        MasterVolume = masterVolume;
        BGMVolume = bGMVolume;
        FXVolume = fXVolume;
        GFXLevel = gFXLevel;
        HapticFeedback = hapticFeedback;
        Language = language;
        Modified = false;
    }
}

public class SettingsManager : MonoBehaviour
{
    enum SETTING_STATE { GENERAL, ADVANCED, INFO, RESET_INFO, RESET_CONFIRM }

    [SerializeField] GameObject[] m_screens = new GameObject[5];
    [SerializeField] Image[] m_buttonSprites = new Image[4];
    [SerializeField] Sprite m_buttonSelected, m_buttonNotSelected;
    [SerializeField] Slider m_masterVolumeSlider, m_bgmSlider, m_fxVolumeSlider, m_gfxQualitySlider;
    [SerializeField] Toggle m_hapticFeedbackToggle;
    [SerializeField] LanguageSelect m_langSelect;
    [SerializeField] SETTING_STATE m_state;

    Settings m_previousSettings, m_currentSettings;

    readonly Settings m_defaults = new Settings(0.8f, 0.8f, 0.8f, 2, true, LANGUAGE.JP);

    public static SettingsManager m_instance;

    void Start ()
    {
        if (PPM.FirstRun())
        {
            PPM.ConfirmFirstRun();
            print("First run confirmed! Setting up initial values");

            SetState((int)SETTING_STATE.GENERAL);

            m_currentSettings = m_previousSettings = m_defaults;

            RefreshDisplayedValues(m_currentSettings);

            OnSaveSettings();
        }

        m_instance = this;

        SetState((int)SETTING_STATE.GENERAL);

        m_previousSettings = new Settings()
        {
            MasterVolume = PPM.LoadFloat(PPM.KEY_FLOAT.VOL_MASTER),
            BGMVolume = PPM.LoadFloat(PPM.KEY_FLOAT.VOL_BGM),
            FXVolume = PPM.LoadFloat(PPM.KEY_FLOAT.VOL_FX),
            GFXLevel = PPM.LoadInt(PPM.KEY_INT.LEVEL_GFX),
            HapticFeedback = PPM.LoadBool(PPM.KEY_BOOL.HAPTIC_FEEDBACK),
            Language = LocalisationManager.GetCurrentLanguage(),
            Modified = false
        };

        m_currentSettings = m_previousSettings;

        RefreshDisplayedValues(m_previousSettings);
    }

    void RefreshDisplayedValues(Settings settings)
    {
        m_masterVolumeSlider.value = settings.MasterVolume;
        m_bgmSlider.value = settings.BGMVolume;
        m_fxVolumeSlider.value = settings.FXVolume;
        m_gfxQualitySlider.value = settings.GFXLevel;
        m_hapticFeedbackToggle.isOn = settings.HapticFeedback;
        m_langSelect.ToggleDisplay(LocalisationManager.GetCurrentLanguage());
    }

    public void SetState(int state)
    {
        m_state = (SETTING_STATE)state;
        UpdateDisplay();
    }

    public void SetActiveButton(Image i)
    {
        foreach (var s in m_buttonSprites)
        {
            s.sprite = m_buttonNotSelected;
        }

        i.sprite = m_buttonSelected;
    }

    public void OnResetToDefault()
    {
        m_currentSettings = m_defaults;
        m_currentSettings.Modified = true;

        LocalisationManager.SetLanguage(m_currentSettings.Language);
        Audio.SetVolume(Audio.AUDIO.MASTER, m_currentSettings.MasterVolume);
        Audio.SetVolume(Audio.AUDIO.BGM, m_currentSettings.BGMVolume);
        Audio.SetVolume(Audio.AUDIO.FX, m_currentSettings.FXVolume);

        RefreshDisplayedValues(m_currentSettings);
    }

    public void OnBack()
    {
        if (!m_currentSettings.Modified) return;
        m_instance.m_currentSettings = m_instance.m_previousSettings;

        LocalisationManager.SetLanguage(m_currentSettings.Language);
        Audio.SetVolume(Audio.AUDIO.MASTER, m_currentSettings.MasterVolume);
        Audio.SetVolume(Audio.AUDIO.BGM, m_currentSettings.BGMVolume);
        Audio.SetVolume(Audio.AUDIO.FX, m_currentSettings.FXVolume);

        m_instance.RefreshDisplayedValues(m_instance.m_currentSettings);
    }

    public void OnSaveSettings()
    {
        PPM.SaveFloat(PPM.KEY_FLOAT.VOL_MASTER, m_currentSettings.MasterVolume);
        PPM.SaveFloat(PPM.KEY_FLOAT.VOL_BGM, m_currentSettings.BGMVolume);
        PPM.SaveFloat(PPM.KEY_FLOAT.VOL_FX, m_currentSettings.FXVolume);
        PPM.SaveInt(PPM.KEY_INT.LEVEL_GFX, m_currentSettings.GFXLevel);
        PPM.SaveBool(PPM.KEY_BOOL.HAPTIC_FEEDBACK, m_currentSettings.HapticFeedback);
        LocalisationManager.SaveLanguage();

        m_previousSettings = m_currentSettings;
        m_currentSettings.Modified = false;
        m_previousSettings.Modified = false;
    }

    public void OnChangeMasterVolume(Slider slider)
    {
        var newVol = slider.value;
        if (newVol == m_currentSettings.MasterVolume) return;
        m_currentSettings.Modified = true;

        m_currentSettings.MasterVolume = newVol;

        Audio.SetVolume(Audio.AUDIO.MASTER, newVol);
    }

    public void OnChangeBGMVolume(Slider slider)
    {
        var newVol = slider.value;
        if (newVol == m_currentSettings.BGMVolume) return;
        m_currentSettings.Modified = true;

        m_currentSettings.BGMVolume = newVol;

        Audio.SetVolume(Audio.AUDIO.BGM, newVol);
    }

    public void OnChangeFXVolume(Slider slider)
    {
        var newVol = slider.value;
        if (newVol == m_currentSettings.FXVolume) return;
        m_currentSettings.Modified = true;

        m_currentSettings.FXVolume = newVol;

        Audio.SetVolume(Audio.AUDIO.FX, newVol);
    }

    public void OnChangeGFXSettings(Slider slider)
    {
        var newVol = (int)slider.value;
        if (newVol == m_currentSettings.GFXLevel) return;
        m_currentSettings.Modified = true;

        m_currentSettings.GFXLevel = newVol;

        // TODO add code to actually change volume
    }

    public void OnToggleHaptic(Toggle slider)
    {
        var on = slider.isOn;
        if (on == m_currentSettings.HapticFeedback) return;
        m_currentSettings.Modified = true;

        m_currentSettings.HapticFeedback = on;

        // TODO add code to actually change this
    }

    public void OnChangeLanguage(int lang)
    {
        if ((LANGUAGE)lang == m_currentSettings.Language) return;
        m_currentSettings.Language = (LANGUAGE)lang;
        m_currentSettings.Modified = true;
    }

    void UpdateDisplay()
    {
        HideAll();

        m_screens[(int)m_state].SetActive(true);
    }

    void HideAll()
    {
        foreach (var screen in m_screens)
        {
            screen.SetActive(false);
            
        }
    }
}
