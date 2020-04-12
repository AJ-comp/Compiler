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
                ObservableCollection<Document> result = new ObservableCollection<Document>
                {
                    new Document(DocumentType.MiniCSource, CommonResource.MiniCFile, CommonResource.MiniCCategory, CommonResource.EmptyMiniCItem, "empty.mc")
                    {
                        Data = "void class()\r\n{\r\n}"
                    },

                    // for test
                    new Document(DocumentType.MiniCHeader, CommonResource.MiniCFile, CommonResource.MiniCCategory, CommonResource.EmptyMiniCItem, "empty.h")
                    {
                        Data = "void interface();"
                    }
                };

                return result;
            }
        }


        public override string ToString() => "MiniC";
    }
}
