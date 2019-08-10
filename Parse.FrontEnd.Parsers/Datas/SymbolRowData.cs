using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.FrontEnd.Parsers.Datas
{
    public class SymbolRowData
    {
        public string Name { get; }
        public uint Type { get; }
        public uint RelativeAddress { get; }
        public uint ArrayLevel { get; }
    }
}
