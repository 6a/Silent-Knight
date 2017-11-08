namespace Entities
{
    interface IEntity
    {
        bool Running { get; set; }

        void Reset();
    }
}
