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
        public TokenDataList NameTokens { get; } = new TokenDataList();

        public List<TypeDefNode> DefTypes { get; } = new List<TypeDefNode>();


        public int Block { get; private set; }

        public List<AJNode> References { get; } = new List<AJNode>();

        public string FullName => NameTokens.ToListString();


//        public NamespaceNode ChildNamespace { get; set; }
    }
}
