using System.Collections.ObjectModel;

namespace ApplicationLayer.Models.GrammarPackages.MiniCPackage
{
    public class MiniCFactory : IGrammarFactory
    {
        public ObservableCollection<NewItem> ItemList
        {
            get
            {
                ObservableCollection<NewItem> result = new ObservableCollection<NewItem>();

                result.Add(new NewItem(string.Empty, "Class", "MiniC Category", "This is a definition that empty class", "Class.cs"));
                result.Add(new NewItem(string.Empty, "Interface", "MiniC Category", "This is a definition that empty interface", "Interface.cs"));

                return result;
            }
        }


        public override string ToString() => "MiniC";
    }
}
