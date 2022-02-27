using Parse.Extensions;
using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes
{
    public class UseIdentNode : ExprNode
    {
        public TokenData IdentToken => AllIdentTokens.Last();
        public TokenDataList AllIdentTokens = new TokenDataList();
        public TokenDataList AllIdentTokensWithoutThis
        {
            get
            {
                TokenDataList result = new TokenDataList();

                foreach (var identToken in AllIdentTokens)
                {
                    if (identToken.IsEqual(AJGrammar.This)) continue;

                    result.Add(identToken);
                }

                return result;
            }
        }
        public VariableAJ Var { get; private set; }
        public FuncDefNode Func { get; private set; }
        public ISymbolData UsedSymbolData => GetSymbol(IdentToken);

        public bool ThisExpression { get; }


        public UseIdentNode(AstSymbol node, bool thisExpression) : base(node)
        {
            ThisExpression = thisExpression;
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
        public override SdtsNode Compile(CompileParameter param)
        {
            base.Compile(param);
            AllIdentTokens.Clear();

            int offset = 0;
            while (true)
            {
                if (offset >= Items.Count) break;

                var node = Items[offset++].Compile(param) as TerminalNode;
                AllIdentTokens.AddExceptNull(node.Token);
            }

            if (IdentToken == null) return this;
            if (!CheckThisKeyword()) return this;
            if (!CheckIsDefinedSymbol(IdentToken)) return this;

            var symbolData = UsedSymbolData;
            if (symbolData is VariableAJ)
            {
                Var = symbolData as VariableAJ;
                if (Var.VariableType == VarType.ReferenceType && !Var.IsInitialized)
                    Alarms.Add(AJAlarmFactory.CreateMCL0005(IdentToken));

                Result = (symbolData as VariableAJ).ToConstantAJ();
            }
            else if (symbolData is FuncDefNode)
            {
                Func = symbolData as FuncDefNode;
                Result = (symbolData as FuncDefNode).ReturnValue;
            }

            //            DBContext.Instance.Insert(this);

            return this;
        }


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
