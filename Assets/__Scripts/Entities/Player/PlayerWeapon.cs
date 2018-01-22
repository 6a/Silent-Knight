using UnityEngine;

namespace SilentKnight.Entities
{
    /// <summary>
    /// Helper component for the players weapon. Handles collision behaviour as well as toggling of collider.
    /// </summary>
    public class PlayerWeapon : MonoBehaviour
    {
        // Reference to player.
        PlayerPathFindingObject m_player;

        // Reference to attached collider.
        BoxCollider m_col;

        void Awake()
        {
            m_player = GetComponentInParent<PlayerPathFindingObject>();
            m_col = GetComponent<BoxCollider>();
        }

        void OnTriggerEnter(Collider other)
        {
            // When the attached collider is enabled and a collision is detected, send the data to the player unit.
            m_player.OnSwordCollision(other.GetComponent<IAttackable>());
        }

        /// <summary>
        /// Toggle the attached collider on and off.
        /// </summary>
        public void Switch(bool on)
        {
            m_col.enabled = on;
        }
    }
}
