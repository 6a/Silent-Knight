using UnityEngine;

namespace Entities
{
    public interface IAttackable
    {
        int ID { get; set; }
        float Health { get; set; }
        int DeathTime { get; set; }
        bool IsDead { get; set; }

        void Damage(IAttacker attacker, float damage, FCT_TYPE type);
        void KnockBack(Vector2 sourcePos, float strength);
        void AfflictStatus(STATUS status, float duration);
        Vector3 Position();
        ITargetable GetTargetableInterface();
    }
}
