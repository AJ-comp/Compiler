using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.StmtExpressions;
using System.Linq;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes
{
    public class CompoundStNode : StatementNode, IRootable
    {
        public StatListNode StatListNode { get; private set; }
        public bool IsRoot => !(Parent is StatementNode);


        public CompoundStNode(AstSymbol node) : base(node)
        {
        }


        // [0] : StatementNode* [StatList] [epsilon able]
        public override SdtsNode Compile(CompileParameter param)
        {
            base.Compile(param);

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
                else item.Compile(param.CloneForNewBlock());
            }

            return this;
        }

        public override IRExpression To()
        {
            var result = new IRCompoundStatement();

            foreach (var localVar in VarList)
                result.LocalVars.Add(localVar.ToIR());

            foreach (var statement in StatListNode.StatementNodes)
                result.Expressions.Add(statement.To());

            return result;
        }

        public override IRExpression To(IRExpression from)
        {
            throw new System.NotImplementedException();
        }
    }
}
