namespace SilentKnight.Entities
{
    /// <summary>
    /// Represents a dynamic entity.
    /// </summary>
    interface IEntity
    {
        // Whether the unit is running.
        bool Running { get; set; }

        // Whether the unit has died or is dead somehow.
        bool IsDead { get; set; }

        // Resets this unit to pre-running state.
        void Reset();
    }
}
