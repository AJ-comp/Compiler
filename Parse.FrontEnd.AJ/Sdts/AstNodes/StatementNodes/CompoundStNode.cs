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
            BlockLevel = param.BlockLevel;

            // it needs to clone an param
            _varList.Clear();
            param.Offset = 0;
            var classDefNode = GetParent(typeof(ClassDefNode)) as ClassDefNode;
            var funcNode = GetParent(typeof(FuncDefNode)) as FuncDefNode;

            foreach (var item in Items)
            {
                if (item is DeclareVarNode)
                {
                    var varNode = item.Compile(param) as DeclareVarNode;
                    _varList.Add(varNode.Variable);
                }
                else if (item is CompoundStNode)
                {
                    // build StatListNode
                    //                    StatListNode = item.Compile(param) as CompoundStNode;
                }
                else if (item is ReturnStatementNode)
                {
                    var returnNode = item.Compile(param) as ReturnStatementNode;
                }
                else if (item is IfStatementNode)
                {
                    var ifNode = item.Compile(param.CloneForNewBlock()) as IfStatementNode;
                }

                if (IsRoot) (param.RootNode as ProgramNode).ShortCutDeclareVarSet.Add(this);
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
