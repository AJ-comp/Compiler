using System.Collections.ObjectModel;

using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.Models.GrammarPackages.MiniCPackage
{
    public class MiniCFactory : IGrammarFactory
    {
        public ObservableCollection<Document> ItemList
        {
            get
            {
                ObservableCollection<Document> result = new ObservableCollection<Document>();

                result.Add(new Document(string.Empty, "Class", CommonResource.MiniCCategory, CommonResource.EmptyMiniCItem, "Class.mc")
                {
                    Data = "void class()\r\n{\r\n}"
                });

                // for test
                result.Add(new Document(string.Empty, "Interface", "MiniC Category", "This is a definition that empty interface", "Interface.mc")
                {
                    Data = "void interface()\r\n{\r\n}"
                });

                return result;
            }
        }


        public override string ToString() => "MiniC";
    }
}
