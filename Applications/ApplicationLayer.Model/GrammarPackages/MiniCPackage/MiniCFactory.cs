using System.Collections.ObjectModel;

namespace ApplicationLayer.Models.GrammarPackages.MiniCPackage
{
    public class MiniCFactory : IGrammarFactory
    {
        public ObservableCollection<Document> ItemList
        {
            get
            {
                ObservableCollection<Document> result = new ObservableCollection<Document>();

                result.Add(new Document(string.Empty, "Class", "MiniC Category", "This is a definition that empty class", "Class.cs"));
                result.Add(new Document(string.Empty, "Interface", "MiniC Category", "This is a definition that empty interface", "Interface.cs"));

                return result;
            }
        }


        public override string ToString() => "MiniC";
    }
}
