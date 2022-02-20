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
        public override SdtsNode Compile(CompileParameter param)
        {
            base.Compile(param);

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
                    else
                    {
                        if (!CheckDeRefable(varData))
                        {
                            Alarms.Add(new MeaningErrInfo(IdentNode.AllTokens,
                                            nameof(AlarmCodes.MCL0019), AlarmCodes.MCL0019));
                        }
                    }
                }
                else // expr case (ex: *(a+b))
                {
                    if (!CheckDeRefable(node.Result))
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

        private bool CheckDeRefable(ISymbolData symbol)
        {
            if (!(symbol is IHasType)) return false;

            var var = symbol as IHasType;

            // deref type has to be address type

            if (var.Type.PointerDepth > 0) return true;
            //            return (var.Type.DataType == AJDataType.Ptr);

            return false;
        }

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
