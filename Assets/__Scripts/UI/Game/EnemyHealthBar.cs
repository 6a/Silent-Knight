using UnityEngine;

namespace SilentKnight.UI.Game
{
    /// <summary>
    /// Healhbar component for enemy healthbars. Derives from HealthBar.
    /// </summary>
    public class EnemyHealthBar : HealthBar
    {
        // Refernces to all UI components, for toggling on and off.
        [SerializeField] GameObject[] cells;

        /// <summary>
        /// Toggle the visibility for this enemy health bar.
        /// </summary>
        public void ToggleVisibility(bool on)
        {
            foreach (var c in cells)
            {
                c.SetActive(on);
            }
        }
    }
}