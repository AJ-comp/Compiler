using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.StatementNodes
{
    public class CompoundStNode : StatementNode
    {
        public VariableDclsListNode VarListNode { get; private set; }
        public StatListNode StatListNode { get; private set; }


        public CompoundStNode(AstSymbol node) : base(node)
        {
        }


        // [0] : VariableDclsListNode [DclList]
        // [1] : StatListNode [StatList] [epsilon able]
        public override SdtsNode Build(SdtsParams param)
        {
            SymbolTable = (param as MiniCSdtsParams).SymbolTable;

            // build VariableDclsListNode
            VarListNode = Items[0].Build(param) as VariableDclsListNode;

            // build StatListNode
            if(Items.Count > 1)
                StatListNode = Items[1].Build(param) as StatListNode;

            return this;
        }
    }
}
