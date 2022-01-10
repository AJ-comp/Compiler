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
            BlockLevel = ParentBlockLevel + 1;

            // it needs to clone an param
            _varList.Clear();
            var newParam = param.Clone();
            newParam.Offset = 0;
            var classDefNode = GetParent(typeof(ClassDefNode)) as ClassDefNode;

            foreach (var item in Items)
            {
                var statementNode = item.Compile(newParam);

                if (statementNode is DeclareVarNode)
                {
                    var varNode = statementNode as DeclareVarNode;
                    _varList.Add(varNode.Variable);
                }
                else if (item is StatListNode)
                {
                    // build StatListNode
                    StatListNode = item.Compile(newParam) as StatListNode;
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
