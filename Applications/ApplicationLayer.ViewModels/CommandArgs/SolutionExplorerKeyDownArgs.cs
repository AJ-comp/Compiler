using ApplicationLayer.Models.SolutionPackage;

namespace ApplicationLayer.ViewModels.CommandArgs
{
    public class SolutionExplorerKeyDownArgs
    {
        public enum PressedKey { F2, Esc, Enter };

        public HierarchicalData Item { get; }
        public PressedKey Key { get; }

        public SolutionExplorerKeyDownArgs(HierarchicalData item, PressedKey key)
        {
            Item = item;
            Key = key;
        }
    }
}
