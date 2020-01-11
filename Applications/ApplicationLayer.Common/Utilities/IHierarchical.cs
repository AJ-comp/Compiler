using System.Collections.ObjectModel;

namespace ApplicationLayer.Common.Utilities
{
    interface IHierarchical<T>
    {
        ObservableCollection<T> Items { get; }
    }
}
