using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.StatementNodes
{
    public class CompoundStNode : StatementNode
    {
        public VariableDclsNode VarListNode { get; private set; }
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

            foreach (var item in Items)
            {
                if (item is VariableDclsNode)
                {
                    // build VariableDclsListNode
                    VarListNode = item.Build(newParam) as VariableDclsNode;
                }
                else if (item is StatListNode)
                {
                    // build StatListNode
                    StatListNode = item.Build(newParam) as StatListNode;
                }
            }

            if (VarListNode != null)
            {
                foreach (var varData in VarListNode.VarList)
                {
                    _varList.Add(varData);
                }
            }

            return this;
        }
    }
}
