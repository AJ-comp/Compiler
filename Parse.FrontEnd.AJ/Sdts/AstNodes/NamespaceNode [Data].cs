using Parse.Extensions;
using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public partial class NamespaceNode
    {
        public List<TokenData> NameTokens { get; set; } = new List<TokenData>();
        public List<ClassDefNode> Classes { get; set; } = new List<ClassDefNode>();
        public int Block { get; private set; }
        public int Offset { get; internal set; }

        public List<AJNode> References { get; } = new List<AJNode>();

        public string FullName => NameTokens.ItemsString(PrintType.String, string.Empty, ".");


//        public NamespaceNode ChildNamespace { get; set; }
    }
}
