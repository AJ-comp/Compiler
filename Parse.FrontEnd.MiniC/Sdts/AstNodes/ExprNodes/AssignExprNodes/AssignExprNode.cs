using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.LiteralNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables;
using Parse.FrontEnd.Grammars.Properties;
using Parse.FrontEnd.MiniC.Properties;
using System;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.AssignExprNodes
{
    public abstract class AssignExprNode : BinaryExprNode
    {
        public VariableMiniC LeftVar { get; private set; }

        public event EventHandler RightIsLiteralHandler;
        public event EventHandler RightIsVarHandler;
        public event EventHandler RightIsExprHandler;

        public AssignExprNode(AstSymbol node) : base(node)
        {
            IsNeedWhileIRGeneration = true;
        }

        public override SdtsNode Build(SdtsParams param)
        {
            base.Build(param);

            if (Left is UseIdentNode)
            {
                LeftVar = (Left as UseIdentNode).VarData;

                if (CanAssign(LeftVar, Right)) Process();
            }
            else
            {
                AddMCL0004Exception();
            }

            return this;
        }

        protected void AddMCL0004Exception()
        {
            Left.ConnectedErrInfoList.Add
            (
                new MeaningErrInfo(Left.MeaningTokens,
                                                nameof(AlarmCodes.MCL0004),
                                                string.Format(AlarmCodes.MCL0004))
            );
        }

        protected bool CanAssign(VariableMiniC leftVar, ExprNode rightNode)
        {
            if (leftVar.GetType() == rightNode.GetType()) return true;
            else
            {
                // check detaily

                return false;
            }
        }


        /// <summary>
        /// This function defines the process for when left is variable.
        /// </summary>
        private void Process()
        {
            if (Right is LiteralNode)    // variable = literal
            {
                RightIsLiteralHandler?.Invoke(this, null);
            }
            else if (Right is UseIdentNode)    // variable = variable
            {
                RightIsVarHandler?.Invoke(this, null);
            }
            else if (Right is ExprNode)   // variable = (exp)
            {
                RightIsExprHandler?.Invoke(this, null);
            }
        }
    }
}
