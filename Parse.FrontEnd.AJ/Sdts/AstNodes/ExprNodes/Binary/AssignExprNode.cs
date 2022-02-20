using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Single;
using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Binary
{
    public abstract class AssignExprNode : BinaryExprNode
    {
        public VariableAJ LeftVar { get; private set; }

        public AssignExprNode(AstSymbol node) : base(node)
        {
            IsNeedWhileIRGeneration = true;
        }

        public override SdtsNode Compile(CompileParameter param)
        {
            base.Compile(param);

            if (!IsCanParsing) return this;

            if (LeftNode is UseIdentNode)
            {
                LeftVar = (LeftNode as UseIdentNode).Var;

                // case for not declared.
                if (LeftVar is null)
                {
                    LeftNode.Alarms.Add(AJAlarmFactory.CreateMCL0001((LeftNode as UseIdentNode).IdentToken));
                    IsCanParsing = false;
                }
                else if (LeftVar.Type.Const)
                {
                    Alarms.Add(AJAlarmFactory.CreateMCL0002(LeftVar.NameToken));
                    IsCanParsing = false;
                }
            }
            else if (LeftNode is DeRefExprNode)
            {
            }
            else
            {
                LeftNode.Alarms.Add(AJAlarmFactory.CreateMCL0004());
                IsCanParsing = false;
            }

            return this;
        }
    }
}
