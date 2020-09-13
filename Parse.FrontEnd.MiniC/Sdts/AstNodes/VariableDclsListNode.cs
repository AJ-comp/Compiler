using Parse.FrontEnd.Ast;
using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes
{
    public class VariableDclsListNode : MiniCNode
    {
        public IReadOnlyList<VariableDclsNode> VarNodes => _vars;

        public VariableDclsListNode(AstSymbol node) : base(node)
        {
        }


        // format summary [Can induce epsilon]
        // [0:n] : VariableDclNode [Dcl]
        public override SdtsNode Build(SdtsParams param)
        {
            foreach (var item in Items)
            {
                SdtsNode cItem = item as VariableDclsNode;
                var result = cItem.Build(param) as VariableDclsNode;
                _vars.Add(result);
            }

            return this;
        }


        private List<VariableDclsNode> _vars = new List<VariableDclsNode>();
    }
}
