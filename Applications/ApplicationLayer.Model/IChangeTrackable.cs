namespace ApplicationLayer.Models
{
    public interface IChangeTrackable
    {
        bool IsChanged { get; }

        void RollBack();
        void Commit();
    }
}
