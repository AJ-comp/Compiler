using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.StmtExpressions;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes
{
    public class CompoundStNode : StatementNode, IRootable
    {
        //        public StatListNode StatListNode { get; private set; }
        public List<StatementNode> StatementNodes = new List<StatementNode>();
        public bool IsRoot => !(Parent is StatementNode);

        public List<object> OrderingItems { get; } = new List<object>();


        public CompoundStNode(AstSymbol node) : base(node)
        {
        }

        public CompoundStNode(int blockLevel, int offset, AJNode parentNode) : base(null)
        {
            Parent = parentNode;
            StubCode = true;

            var compileParam = new CompileParameter
            {
                BlockLevel = blockLevel,
                Offset = offset,
                RootNode = parentNode.RootNode,
            };

            base.CompileLogic(compileParam);
        }


        // [0] : StatementNode* [StatList] [epsilon able]
        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            base.CompileLogic(param);

            // it needs to clone an param
            _varList.Clear();
            param.Offset = 0;
            var classDefNode = GetParent(typeof(ClassDefNode)) as ClassDefNode;
            var funcNode = GetParent(typeof(FuncDefNode)) as FuncDefNode;

            foreach (var item in Items)
            {
                if (item is DeclareVarStNode)
                {
                    var varNode = item.Compile(param) as DeclareVarStNode;
                    _varList.UnionWith(varNode.VarList);
                }
                else if (item is CompoundStNode)
                {
                    // build StatListNode
                    //                    StatListNode = item.Compile(param) as CompoundStNode;
                }
                else if (item is ReturnStatementNode)
                {
                    item.Compile(param);
                    ClarifyReturn = true;
                }
                else StatementNodes.Add(item.Compile(param.CloneForNewBlock()) as StatementNode);

                OrderingItems.Add(item);
            }

            return this;
        }

        public override IRExpression To()
        {
            var result = new IRCompoundStatement();

            foreach (var item in Items)
            {
                var statement = item as StatementNode;

                result.Items.Add(statement.To());
            }

            return result;
        }

        public override IRExpression To(IRExpression from)
        {
            throw new System.NotImplementedException();
        }
    }
}
