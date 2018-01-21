using Entities;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Component helper for XP bars.
/// </summary>
public class XPBar : MonoBehaviour
{
    // The UI object of the xp bar that acts as the fill area.
    [SerializeField] RectTransform m_fill;

    // The text object for this xp bar, which should display xp information.
    [SerializeField] Text m_xpText;

    // Text objects for this xp bar, which should display the current, and next level (left and right respectively).
    [SerializeField] Text m_currentLevelText, m_nextLevelText;

    // The width of the xp bar - should be calculated on Awake().
    float m_width;

    void Awake()
    {
        // Get the width of the xp bar.
        m_width = m_fill.sizeDelta.x;
    }

    // OnEnable is used to prevent some lag on startup - The xpbar does not need to be updated until the bonus window is turned on.
    void OnEnable()
    {
        UpdateXPBar();
    }

    /// <summary>
    /// Updates the xp bar with values taken from persistent data.
    /// </summary>
    public void UpdateXPBar()
    {
        // Get current xp from persistent data.
        var currentXP = PersistentData.LoadInt(PersistentData.KEY_INT.XP);

        // Calculate current level using current xp.
        var currentLevel = LevelScaling.GetLevel(currentXP);

        // Calculate xp for next level.
        var xpForNextLevel = LevelScaling.GetXP(currentLevel + 1);

        // Calculate xp for previous level.
        var xpForPreviousLevel = LevelScaling.GetXP(currentLevel);

        // Calculate progress through current level in terms of xp.
        var xpThroughCurrentLevel = currentXP - xpForPreviousLevel;

        // Calculate xp gap between this and the next level.
        var xpForWholeLevel = xpForNextLevel - xpForPreviousLevel;

        // Calculate current progress through this level as a value between 0-1.
        var fractionOfCurrentLevel = Mathf.Clamp01(xpThroughCurrentLevel / (float)xpForWholeLevel);

        // Resize the xp bar accordingly.
        m_fill.sizeDelta = new Vector2(fractionOfCurrentLevel * m_width, m_fill.sizeDelta.y);

        // Update the xp bar text accordingly.
        m_xpText.text = xpThroughCurrentLevel + "XP / " + xpForWholeLevel + "XP";

        // Update the current and next level labels accordingly.
        m_currentLevelText.text = "LVL " + currentLevel;
        m_nextLevelText.text = "LVL " + (currentLevel + 1);
    }
}
