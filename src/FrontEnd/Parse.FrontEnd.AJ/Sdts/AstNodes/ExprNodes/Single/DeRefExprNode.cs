using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using System;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Single
{
    /// <summary>
    /// *a
    /// *A()
    /// </summary>
    public class DeRefExprNode : SingleExprNode
    {
        public DeRefExprNode(AstSymbol node) : base(node)
        {
        }


        // * Expr
        // [1] : ExprNode
        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            base.CompileLogic(param);

            var node = Items[0].Compile(param) as ExprNode;

            try
            {
                if (node is UseIdentNode)
                {
                    var IdentNode = node as UseIdentNode;
                    var varData = GetSymbol(IdentNode.IdentToken);
                    if (varData == null)
                    {
                        // if varData is null it is that IdentNode.IdentToken is not declared.
                        Alarms.Add(new MeaningErrInfo(IdentNode.AllTokens,
                                                    nameof(AlarmCodes.MCL0001),
                                                    string.Format(AlarmCodes.MCL0001, IdentNode.IdentToken)));
                    }
                    else if(varData is VariableAJ)
                    {
                        var variable = varData as VariableAJ;

                        if (!CheckDeRefable(variable.Type))
                        {
                            Alarms.Add(new MeaningErrInfo(IdentNode.AllTokens,
                                            nameof(AlarmCodes.MCL0019), AlarmCodes.MCL0019));
                        }
                    }
                    else if(varData is FuncDefNode)
                    {
                        var func = varData as FuncDefNode;

                        if (!CheckDeRefable(func.ReturnType))
                        {
                            Alarms.Add(new MeaningErrInfo(IdentNode.AllTokens,
                                            nameof(AlarmCodes.MCL0019), AlarmCodes.MCL0019));
                        }
                    }
                }
                else // expr case (ex: *(a+b))
                {
                    if (!CheckDeRefable(node.Type))
                    {
                        Alarms.Add(new MeaningErrInfo(node.AllTokens,
                                        nameof(AlarmCodes.MCL0019), AlarmCodes.MCL0019));
                    }
                }
            }
            catch(Exception)
            {

            }
            finally
            {
                if (RootNode.IsBuild) DBContext.Instance.Insert(this);
            }

            return this;
        }

        private bool CheckDeRefable(AJType type) => type.PointerDepth > 0;

        public override IRExpression To()
        {
            throw new System.NotImplementedException();
        }

        public override IRExpression To(IRExpression from)
        {
            throw new System.NotImplementedException();
        }
    }
}
