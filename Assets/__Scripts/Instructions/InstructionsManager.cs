using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SilentKnight.Utility;

namespace SilentKnight.Tutorial
{
    /// <summary>
    /// Handles the tutorial slideshow at the start of the game (only on first run).
    /// </summary>
    public class InstructionsManager : MonoBehaviour
    {
        // References to the images representing each language. 0 = JP, 1 = EN.
        [SerializeField] Image[] m_languageIcons;

        // References to the two different main slides of the instruction screen.
        [SerializeField] GameObject m_languageSelectScreen, m_demoScreen;

        // References to each slide/text combo.
        [SerializeField] GameObject[] m_demoSlides;
        [SerializeField] GameObject[] m_demoTexts;

        // Current index of the demo slides.
        int m_demoIndex;

        void Awake()
        {
            // If this is not the first run, skip this process and load the main game.
            //if (!PersistentData.FirstRun()) SceneManager.LoadSceneAsync(1);

            m_demoIndex = 0;

            // If this is the first run, set the correct flag for the default language.

            //if ((Enums.LANGUAGE)PersistentData.LoadInt(PersistentData.KEY_INT.LANGUAGE) == Enums.LANGUAGE.EN)
            //{
            //    OnClickENFlag();
            //}
            //else
            //{
            //    OnClickJPFlag();
            //}

            // As this version of the game is for marking purposes, the default langauge is English.
            OnClickENFlag();
        }

        /// <summary>
        /// Button - Enables the demo slide screens.
        /// </summary>
        public void OnClickStart()
        {
            m_languageSelectScreen.SetActive(false);
            m_demoScreen.SetActive(true);
        }

        /// <summary>
        /// Toggles langauge to Japanese and saves the value to persistent data.
        /// </summary>
        public void OnClickJPFlag()
        {
            var enCol = m_languageIcons[0].color;
            enCol.a = 64f / 255f;
            m_languageIcons[1].color = enCol;

            var jpCol = m_languageIcons[0].color;
            jpCol.a = 1;
            m_languageIcons[0].color = jpCol;

            PersistentData.SaveInt(PersistentData.KEY_INT.LANGUAGE, (int)Enums.LANGUAGE.JP);
        }

        /// <summary>
        /// Toggles langauge to English and saves the value to persistent data.
        /// </summary>
        public void OnClickENFlag()
        {
            var jpCol = m_languageIcons[0].color;
            jpCol.a = 64f / 255f;
            m_languageIcons[0].color = jpCol;

            var enCol = m_languageIcons[0].color;
            enCol.a = 1;
            m_languageIcons[1].color = enCol;

            PersistentData.SaveInt(PersistentData.KEY_INT.LANGUAGE, (int)Enums.LANGUAGE.EN);
        }

        /// <summary>
        /// Button - Advances the tutorial slideshow, and triggers the main game load when the tutorial has finished.
        /// </summary>
        public void OnMoveToNextDemoSlide()
        {
            m_demoIndex++;

            if (m_demoIndex == m_demoSlides.Length)
            {
                SceneManager.LoadSceneAsync(1);
                return;
            }
            else if (m_demoIndex > m_demoSlides.Length)
            {
                return;
            }

            m_demoSlides[m_demoIndex].SetActive(true);
            m_demoSlides[m_demoIndex - 1].SetActive(false);

            m_demoTexts[m_demoIndex].SetActive(true);
            m_demoTexts[m_demoIndex - 1].SetActive(false);
        }
    }
}