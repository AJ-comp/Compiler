using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.FrontEnd.Grammars.MiniC.AstNodes
{
    public class DclSpecNode
    {
        public bool Const => (ConstNode == null) ? false : true;
        public DataType DataType { get; internal set; }

        public ConstNode ConstNode { get; internal set; } = null;
        public TokenData DataTypeToken { get; internal set; }

        public string KeyString => string.Empty;

        public override string ToString() => string.Format("const : {0}, DataType : {1}", Const.ToString(), DataType.ToString());
    }
}
