using SilentKnight.Utility;

namespace SilentKnight.UI.Settings
{
    /// <summary>
    /// Storage container for settings data.
    /// </summary>
    public struct Settings
    {
        public float MasterVolume, BGMVolume, SFXVolume;
        public int GFXLevel;
        public bool HapticFeedback;
        public Enums.LANGUAGE Language;
        public bool Modified;

        public Settings(float masterVolume, float bGMVolume, float fXVolume, int gFXLevel, bool hapticFeedback, Enums.LANGUAGE language)
        {
            MasterVolume = masterVolume;
            BGMVolume = bGMVolume;
            SFXVolume = fXVolume;
            GFXLevel = gFXLevel;
            HapticFeedback = hapticFeedback;
            Language = language;
            Modified = false;
        }
    }
}
