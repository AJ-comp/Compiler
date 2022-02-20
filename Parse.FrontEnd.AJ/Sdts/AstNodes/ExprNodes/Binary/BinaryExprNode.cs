using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LiteralNodes;
using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Binary
{
    public abstract class BinaryExprNode : ExprNode
    {
        public ExprNode LeftNode { get; private set; }
        public ExprNode RightNode { get; private set; }

        public bool IsBothLiteral => LeftNode is LiteralNode && RightNode is LiteralNode;
        public bool IsCanParsing { get; protected set; } = true;


        protected BinaryExprNode(AstSymbol node) : base(node)
        {
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

            if (LeftNode.Result == null || RightNode.Result == null)
            {
                Alarms.Add(new MeaningErrInfo(AllTokens,
                                                                nameof(AlarmCodes.AJ0036),
                                                                string.Format(AlarmCodes.AJ0036, Ast.ConnectedParseTree.AllInputDatas)));

                RootNode.UnLinkedSymbols.Add(this);

                result = false;
            }

            return result;
        }
    }
}
