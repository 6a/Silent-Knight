using UnityEngine;

namespace Entities
{
    public interface IAttackable
    {
        int ID { get; set; }
        float Health { get; set; }
        int DeathTime { get; set; }
        bool IsDead { get; set; }
        bool IsBossUnit { get; set; }

        void OnDamage(IAttacker attacker, float damage, FCT_TYPE type);
        void OnKnockBack(Vector2 sourcePos, float strength);
        void OnAfflictStatus(STATUS status, float duration);
        void OnSetAsRenderTarget(bool on);

        Vector3 GetPosition();
        ITargetable GetTargetableInterface();

    }
}
