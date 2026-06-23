using System.Collections.ObjectModel;

namespace ApplicationLayer.Models.GrammarPackages
{
    public interface IGrammarFactory
    {
        ObservableCollection<Document> ItemList { get; }
    }
}
