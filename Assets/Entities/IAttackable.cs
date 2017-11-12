using UnityEngine;

namespace Entities
{
    public interface IAttackable
    {
        float Health { get; set; }
        int DeathTime { get; set; }
        bool IsDead { get; set; }

        void Damage(IAttacker attacker, float damage);
        void KnockBack();
        void AfflictStatus(STATUS status);
        Vector3 Position();
        ITargetable GetTargetableInterface();
    }
}
