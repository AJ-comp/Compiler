using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LiteralNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Single;
using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Binary
{
    public abstract class BinaryExprNode : ExprNode
    {
        public ExprNode LeftNode { get; protected set; }
        public ExprNode RightNode { get; protected set; }

        public bool IsBothLiteral => LeftNode is LiteralNode && RightNode is LiteralNode;
        public bool IsCanParsing { get; protected set; } = true;


        protected BinaryExprNode(AstSymbol node) : base(node)
        {
        }

        protected void CheckAssignable()
        {
            if (!IsCanParsing) return;

            if (LeftNode is UseIdentNode)
            {
                var LeftVar = (LeftNode as UseIdentNode).Var;

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
        }


        public override SdtsNode Compile(CompileParameter param)
        {
            base.Compile(param);

            // ExprNode or TerminalNode
            LeftNode = Items[0].Compile(param) as ExprNode;
            RightNode = Items[1].Compile(param) as ExprNode;

            IsCanParsing = CheckIfCorrectExpression();

            return this;
        }


        public bool CheckIfCorrectExpression()
        {
            bool result = true;

            if (LeftNode.Type == null || RightNode.Type == null)
            {
                Alarms.Add(new MeaningErrInfo(AllTokens,
                                                                nameof(AlarmCodes.AJ0036),
                                                                string.Format(AlarmCodes.AJ0036, Ast.ConnectedParseTree.AllInputDatas)));

                result = false;
            }

            return result;
        }
    }
}
