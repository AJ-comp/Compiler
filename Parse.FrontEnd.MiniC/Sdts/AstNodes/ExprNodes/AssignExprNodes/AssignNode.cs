using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.LiteralNodes;
using Parse.FrontEnd.MiniC.Sdts.Datas.Variables;
using Parse.MiddleEnd.IR.Datas;
using Parse.Types.ConstantTypes;
using System;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.AssignExprNodes
{
    public class AssignNode : AssignExprNode
    {
        public AssignNode(AstSymbol node) : base(node)
        {
            RightIsLiteralHandler += AssignNode_RightIsLiteralHandler;
            RightIsVarHandler += AssignNode_RightIsVarHandler;
            RightIsExprHandler += AssignNode_RightIsExprHandler;
        }


        /// <summary>
        /// This function defines the process for when left is variable and right is literal.
        /// </summary>
        private void AssignNode_RightIsLiteralHandler(object sender, EventArgs e)
        {
            var leftVarData = (Left as UseIdentNode).VarData;

            if (Right is IntLiteralNode)
            {
                var rightLiteral = Right as IntLiteralNode;

                leftVarData.Assign(rightLiteral.LiteralData);
            }
        }


        /// <summary>
        /// This function defines the process for when both left and right is variable.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AssignNode_RightIsVarHandler(object sender, EventArgs e)
        {
            var rightVarData = (Right as UseIdentNode).VarData;

            if (rightVarData != null)
            {
                LeftVar.Assign(rightVarData);
            }
        }


        /// <summary>
        /// This function defines the process for when left is variable and right is expr.
        /// </summary>
        private void AssignNode_RightIsExprHandler(object sender, EventArgs e)
        {
            try
            {
                Result = LeftVar.Assign(Right.Result);
            }
            catch (FormatException ex)
            {

            }
            catch (NotSupportedException ex)
            {

            }
        }
    }
}
