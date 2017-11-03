namespace Entities
{
    interface IAttackable
    {
        int Health { get; set; }
        int DeathTime { get; set; }
        bool IsDead { get; set; }

        void Damage();
        void KnockBack();
        void AfflictStatus(STATUS status);
    }
}
