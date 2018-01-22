using UnityEngine;

namespace SilentKnight.UI.Bonus
{
    /// <summary>
    /// Convenience class that allows for external enabling/disabling of a Link button's animation object (button on the bottom toolbar).
    /// </summary>
    public class LinkButton : MonoBehaviour
    {
        [SerializeField] GameObject m_liveObject;

        /// <summary>
        /// Toggle the button's animation on (true) or off (false).
        /// </summary>
        public void Toggle(bool on)
        {
            m_liveObject.SetActive(on);
        }
    }
}