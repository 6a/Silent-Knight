namespace Entities
{
    interface IAttacker
    {
        IAttackable CurrentTarget { get; set; }

        void Attack(IAttackable target);
        void GetInRange(ITargetable target);
        void AfflictStatus(IAttackable target);
    }
}
