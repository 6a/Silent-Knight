namespace Entities
{
    /// <summary>
    /// A unit that can attack
    /// </summary>
    public interface IAttacker
    {
        void OnTargetDied(IAttackable target, bool boss = true);

        UnityEngine.Vector3 GetPosition();
    }
}
