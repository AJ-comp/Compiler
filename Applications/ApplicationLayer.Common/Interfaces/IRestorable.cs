namespace ApplicationLayer.Common.Interfaces
{
    public interface IRestorable
    {
        bool IsChanged { get; }

        void SyncWithLoadValue();
        void SyncWithCurrentValue();
    }
}
