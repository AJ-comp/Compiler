using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.StatementNodes
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
            // it needs to clone an param
            var newParam = CreateParamForNewBlock(param);

            // build VariableDclsListNode
            VarListNode = Items[0].Build(newParam) as VariableDclsListNode;

            // build StatListNode
            if(Items.Count > 1)
                StatListNode = Items[1].Build(newParam) as StatListNode;

            return this;
        }
    }
}
