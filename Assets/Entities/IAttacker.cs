namespace Entities
{
    interface IAttacker
    {
        IAttackable CurrentTarget { get; set; }

        void Attack(IAttackable target);
        void GetInRange(IAttackable target);
        void AfflictStatus(IAttackable target);
    }
}
