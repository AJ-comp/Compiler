using System.Collections.ObjectModel;

namespace ApplicationLayer.Common.Interfaces
{
    public interface IHasableChildren<T>
    {
        ObservableCollection<T> Children { get; }
    }
}
