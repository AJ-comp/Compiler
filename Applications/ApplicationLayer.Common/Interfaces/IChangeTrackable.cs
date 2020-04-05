namespace ApplicationLayer.Common.Interfaces
{
    public interface IChangeTrackable
    {
        bool IsChanged { get; }

        void SyncWithLoadValue();
        void SyncWithCurrentValue();
    }
}
