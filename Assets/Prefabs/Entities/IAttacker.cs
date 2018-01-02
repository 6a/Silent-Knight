namespace Entities
{
    public interface IAttacker
    {
        IAttackable CurrentTarget { get; set; }

        void Attack(IAttackable target);
        void GetInRange(ITargetable target);
        void AfflictStatus(IAttackable target);

        void OnTargetDied(IAttackable target);

        UnityEngine.Vector3 GetWorldPos();
    }
}
