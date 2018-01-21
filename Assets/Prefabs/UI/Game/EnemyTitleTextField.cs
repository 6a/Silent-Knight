namespace Localisation
{
    /// <summary>
    /// Enemy text field helped component - Derives from TextField.
    /// </summary>
    public class EnemyTitleTextField : TextField
    {
        /// <summary>
        /// Updates the text field data for both languages.
        /// </summary>
        public void UpdateTextData(string en, string jp)
        {
            m_data[0] = en;
            m_data[1] = jp;
            SetLanguage(m_currentlanguage);
        }
    }
}
