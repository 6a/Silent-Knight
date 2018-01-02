namespace Localisation
{
    public class EnemyTitleTextField : TextField
    {
        public void UpdateTextData(string en, string jp)
        {
            m_data[0] = en;
            m_data[1] = jp;
            SetLanguage(m_currentlanguage);
        }
    }
}
