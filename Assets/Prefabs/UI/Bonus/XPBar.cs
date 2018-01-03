using Entities;
using UnityEngine;
using UnityEngine.UI;

public class XPBar : MonoBehaviour
{
    [SerializeField] RectTransform m_fill;
    [SerializeField] Text m_xpText;
    [SerializeField] Text m_currentLevelText, m_nextLevelText;
    float m_width;

    void Awake()
    {
        m_width = m_fill.sizeDelta.x;
    }

    void OnEnable()
    {
        UpdateXPBar();
    }

    public void UpdateXPBar()
    {
        var currentXP = PPM.LoadInt(PPM.KEY_INT.XP);

        var currentLevel = LevelScaling.GetLevel(currentXP);

        var xpForNextLevel = LevelScaling.GetXP(currentLevel + 1);

        var xpForPreviousLevel = LevelScaling.GetXP(currentLevel);

        var xpThroughCurrentLevel = currentXP - xpForPreviousLevel;

        var xpForWholeLevel = xpForNextLevel - xpForPreviousLevel;

        var fractionOfCurrentLevel = Mathf.Clamp01(xpThroughCurrentLevel / xpForWholeLevel);

        m_fill.sizeDelta = new Vector2(fractionOfCurrentLevel * m_width, m_fill.sizeDelta.y);

        m_xpText.text = xpThroughCurrentLevel + "XP / " + xpForWholeLevel + "XP";

        m_currentLevelText.text = "LVL " + currentLevel;

        m_nextLevelText.text = "LVL " + (currentLevel + 1);
    }
}
