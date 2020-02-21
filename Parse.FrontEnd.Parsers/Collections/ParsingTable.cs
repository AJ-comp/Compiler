using System.Data;

namespace Parse.FrontEnd.Parsers.Collections
{
    public interface IParsingTable
    {
        string Introduce { get; set; }

        DataTable ToTableFormat { get; }

        void CreateParsingTable(params object[] datas);
    }
}
