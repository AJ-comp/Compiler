using Parse.Extensions;
using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes
{
    public class CallNode : ExprNode, ICanbeStatement
    {
        public FuncDefNode Func { get; private set; }

        public TokenDataList MethodNameTokens { get; private set; } = new TokenDataList();
        public TokenData MethodNameToken { get; private set; }
        public List<ExprNode> Params { get; } = new List<ExprNode>();

        public CallNode(AstSymbol node) : base(node)
        {
            IsNeedWhileIRGeneration = true;
        }



        // [0] : Ident [UseIdentNode]
        // [1] : ActualParam? (AstNonTerminal)
        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            base.CompileLogic(param);

            var typeDefNode = GetParentAs(typeof(TypeDefNode)) as TypeDefNode;

            int offset = 0;
            var functionName = Items[offset++].Compile(param) as TerminalNode;
            MethodNameToken = functionName.Token;

            while (Items.Count > offset)
            {
                var result = Items[offset++].Compile(param) as ActualParamNode;
                Params.AddRange(result.ParamNodeList);
            }

            Func = GetFuncDefForCallInfo(typeDefNode);
            Type = Func?.ReturnType;

            return this;
        }

        private IEnumerable<FuncDefNode> GetParamCountMatchedList(IEnumerable<FuncDefNode> funcListToFind)
        {
            List<FuncDefNode> result = new List<FuncDefNode>();

            foreach (var func in funcListToFind)
            {
                if (func.ParamVarList.Count() != Params.Count()) continue;

                result.Add(func);
            }

            return result;
        }


        private int GetMatchedTypeCount(IEnumerable<VariableAJ> toCompareList)
        {
            if (toCompareList.Count() != Params.Count) return 0;

            int result = 0;
            for (int i = 0; i < toCompareList.Count(); i++)
            {
                try
                {
                    var src = toCompareList.ElementAt(i);
                    var target = Params[i];

                    if (src.Type != Params[i].Type) break;

                    result++;
                }
                catch
                {
                    break;
                }
            }

            return result;
        }


        public void AddCantFunc()
        {
            List<string> fullTypeName = new List<string>();

            foreach (var param in Params) fullTypeName.Add(param.Type.DefineNode.FullName);

            Alarms.Add(new MeaningErrInfo(MethodNameToken,
                                                            nameof(AlarmCodes.AJ0044),
                                                            string.Format(AlarmCodes.AJ0044, fullTypeName.ItemsString(PrintType.String), MethodNameToken.Input)));
        }

        public void AddNotExistFunc(TypeDefNode lastTypeNode)
        {
            Alarms.Add(new MeaningErrInfo(MethodNameToken,
                                                nameof(AlarmCodes.AJ0045),
                                                string.Format(AlarmCodes.AJ0045, lastTypeNode.FullName, MethodNameToken.Input)));
        }


        public FuncDefNode GetFuncDefForCallInfo(TypeDefNode lastTypeNode)
        {
            FuncDefNode result = null;

            var candidate = lastTypeNode.AllFuncs.Where(x => x.Name == MethodNameToken.Input);
            if (candidate.Count() == 0)
            {
                AddNotExistFunc(lastTypeNode);
                return result;
            }

            candidate = candidate.Where(x => x.ParamVarList.Count() == Params.Count);
            if (candidate.Count() == 0)
            {
                AJAlarmFactory.CreateMCL0015(Params.Count, MethodNameToken);
                return result;
            }

            foreach (var funcDef in candidate)
            {
                int matchCount = 0;

                for (int i = 0; i < funcDef.ParamVarList.Count(); i++)
                {
                    var param = funcDef.ParamVarList[i];
                    if (param.Type.DefineNode == null) break;
                    if (Params[i].Type.DefineNode == null) break;

                    if (param.Type.DefineNode.FullName != Params[i].Type.DefineNode.FullName) break;
                    matchCount++;
                }

                if (matchCount == funcDef.ParamVarList.Count())
                {
                    result = funcDef;
                    break;
                }
            }

            if (result == null) AddCantFunc();

            return result;
        }

        public override IRExpression To()
        {
            List<IRExpr> @params = new List<IRExpr>();
            if (!Func.IsStatic)
            {
                var hostStruct = GetParentAs(typeof(TypeDefNode)) as TypeDefNode;
                @params.Add(new IRUseIdentExpr(hostStruct.ToIRVariable()));
            }

            foreach(var param in Params)
                @params.Add(param.To() as IRExpr);

            return new IRCallExpr(Func.LazyTo() as IRFunction, @params, GetDebuggingData());
        }

        public override IRExpression To(IRExpression from)
        {
            throw new NotImplementedException();
        }
    }
}
