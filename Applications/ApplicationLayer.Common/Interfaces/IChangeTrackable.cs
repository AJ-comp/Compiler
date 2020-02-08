namespace ApplicationLayer.Common.Interfaces
{
    public interface IChangeTrackable : ISaveable
    {
        bool IsChanged { get; }

        void RollBack();
        void Commit();
    }
}
