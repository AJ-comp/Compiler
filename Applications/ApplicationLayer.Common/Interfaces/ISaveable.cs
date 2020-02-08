namespace ApplicationLayer.Common.Interfaces
{
    public interface ISaveable
    {
        string CurOPath { get; set; }
        string FullName { get; set; }
        string NameWithoutExtension { get; }
        string FullPath { get; }

        void Save();
    }
}
