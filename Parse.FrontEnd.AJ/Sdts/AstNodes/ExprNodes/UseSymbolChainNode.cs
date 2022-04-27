using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes
{
    public class UseSymbolChainNode : ExprNode
    {
        public TokenDataList AllSymbolTokens = new TokenDataList();

        public UseSymbolChainNode(AstSymbol node) : base(node)
        {
        }


        /// <summary>
        /// format summary  <br/>
        /// [0] : Ident     <br/>
        /// [1:n] : Ident   (option)    <br/>
        /// This function checks condition as the below <br/>
        /// 1. is it defined?   <br/>
        /// 2. if ident is the reference type variable is it initialized before use?   <br/>
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            base.CompileLogic(param);

            ExprNode lastNode = CompileFirstNode(param);
            if (lastNode.Type == null) return this;

            int offset = 1;
            while (true)
            {
                if (offset >= Items.Count)
                {
                    Type = lastNode.Type;
                    break;
                }

                lastNode = Items[offset++].Compile(param) as ExprNode;
                AllSymbolTokens.AddRange(lastNode.AllTokens);
                if (lastNode.Type == null)
                {
                    AddAlarmUnknownType(AllSymbolTokens);
                    break;
                }
            }

            //            DBContext.Instance.Insert(this);

            return this;
        }


        private ExprNode CompileFirstNode(CompileParameter param)
        {
            FuncDefNode funcDefNode = GetParentAs(typeof(FuncDefNode)) as FuncDefNode;

            TypeDefNode lastTypeNode = GetParentAs(typeof(TypeDefNode)) as TypeDefNode;
            param.ParentNode = lastTypeNode;

            /* UseIdentNode can't analysis var name that included namespace name or type name yet.
            * ex : 
            * a => can analysis
            * Type.a => can't analysis
            * System.Linq.Type.a => can't analysis
            * */

            // first symbol decides whether member var or local var so it has to be treated differently.
            var result = Items[0].Compile(param) as ExprNode;
            AllSymbolTokens.AddRange(result.AllTokens);

            if (Type == null)
            {
                // if first symbol and type is null then UseIdentNode or CallNode is.
                if (result is UseIdentNode)
                {
                    var useIdentNode = result as UseIdentNode;
                    var symbol = useIdentNode.UsedSymbolData;

                    if (symbol == null) AddNotDefinedError(useIdentNode.IdentToken);
                    else result.Type = symbol.Type;
                }
                else if (result is CallNode)
                {
                    var callNode = result as CallNode;

                    if (callNode.Func == null) AddAlarmNoSymbolInContext();
                    else result.Type = callNode.Func.ReturnType;
                }
                else
                {
                    throw new Exception();
                }
            }

            if (result.Type == null) AddAlarmUnknownType(AllSymbolTokens);

            return result;
        }


        /*
        private bool CheckThisKeyword()
        {
            bool result = true;

            if (AllIdentTokens[0].Input == AJGrammar.This.Caption)
            {

            }

            // if there is a this keyword more than 2.
            var tokensWithOutFirst = AllIdentTokens.Skip(1);
            if (tokensWithOutFirst.Where(x => x.Input == AJGrammar.This.Caption).Count() > 0)
            {
                result = false;
                Alarms.Add(new MeaningErrInfo(tokensWithOutFirst, nameof(AlarmCodes.AJ0037), AlarmCodes.AJ0037));
            }

            return result;
        }

        private SdtsNode NotThisCaseCompile(CompileParameter param)
        {
            if (AllIdentTokens.Where(x => x.Input == AJGrammar.This.Caption).Count() > 0)
                Alarms.Add(new MeaningErrInfo(AllIdentTokens, nameof(AlarmCodes.AJ0037), AlarmCodes.AJ0037));

            //            DBContext.Instance.Insert(this);

            return this;
        }
        */

        public override IRExpression To()
        {
            throw new NotImplementedException();
        }

        public override IRExpression To(IRExpression from)
        {
            throw new NotImplementedException();
        }
    }
}
