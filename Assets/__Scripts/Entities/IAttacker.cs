namespace SilentKnight.Entities
{
    /// <summary>
    /// A unit that can attack.
    /// </summary>
    public interface IAttacker
    {
        // Call this function when this unit dies.
        void OnTargetDied(IAttackable target, bool boss = true);

        // Get the position of this unit.
        UnityEngine.Vector3 GetPosition();
    }
}
