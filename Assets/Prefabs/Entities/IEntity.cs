namespace Entities
{
    interface IEntity
    {
        bool Running { get; set; }
        bool IsDead { get; set; }

        void Reset();
    }
}
