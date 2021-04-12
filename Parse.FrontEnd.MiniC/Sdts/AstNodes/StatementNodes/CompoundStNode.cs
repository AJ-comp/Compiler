using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Sdts.Datas;
using Parse.MiddleEnd.IR.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.StatementNodes
{
    public class CompoundStNode : StatementNode, ICompoundStmtExpression
    {
        public StatListNode StatListNode { get; private set; }

        public IEnumerable<IDeclareVarExpression> LocalVars => VarList;
        public IEnumerable<IStmtExpression> Statements => StatementNodes;


        public CompoundStNode(AstSymbol node) : base(node)
        {
        }


        // [0] : VariableDclsListNode [DclList]
        // [1] : StatListNode [StatList] [epsilon able]
        public override SdtsNode Build(SdtsParams param)
        {
            BlockLevel = ParentBlockLevel + 1;

            // it needs to clone an param
            _varList.Clear();
            var newParam = param.Clone();
            var classDefNode = GetParent(typeof(ClassDefNode)) as ClassDefNode;

            foreach (var item in Items)
            {
                if (item is VariableDclListNode)
                {
                    newParam.Offset = (VarList == null) ? 0 : VarList.Count();

                    // build VariableDclsListNode
                    var varListNode = item.Build(newParam) as VariableDclListNode;

                    foreach (var varData in varListNode.VarList)
                    {
//                        varData.PartyName = classDefNode.ClassData.Name;
                        _varList.Add(varData);
                    }
                }
                else if (item is StatListNode)
                {
                    // build StatListNode
                    StatListNode = item.Build(newParam) as StatListNode;
                }
            }

            return this;
        }
    }
}
