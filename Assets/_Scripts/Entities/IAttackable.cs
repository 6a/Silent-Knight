using UnityEngine;

namespace Entities
{
    /// <summary>
    /// A unit that can be attacked.
    /// </summary>
    public interface IAttackable
    {
        // Unique ID number assigned to each unit. Numbers are refreshed each level load.
        int ID { get; set; }

        // Health of the unit.
        float Health { get; set; }

        // Various state identifiers.
        bool IsDead { get; set; }
        bool IsBossUnit { get; set; }

        // Call this function to damage this unit.
        void OnDamage(IAttacker attacker, float damage, Enums.FCT_TYPE type);

        // Call this function to knockback this unit.
        void OnKnockBack(Vector2 sourcePos, float strength);

        // Call this function to afflict this unit with a status.
        void OnAfflict(Enums.AFFLICTION affliction, float duration);

        // Set this unit as the enemy render target (enemy units only).
        void OnSetAsRenderTarget(bool on);

        // Get the position of this unit.
        Vector3 GetPosition();

        // Convert this unit into an ITargetable.
        ITargetable GetTargetableInterface();
    }
}
