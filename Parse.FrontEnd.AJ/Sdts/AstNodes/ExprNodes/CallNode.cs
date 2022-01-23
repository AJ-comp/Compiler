﻿using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes
{
    public class CallNode : ExprNode
    {
        public FuncDefNode Func { get; private set; }

        public TokenData MethodNameToken { get; private set; }
        public List<ExprNode> Params { get; } = new List<ExprNode>();

        public CallNode(AstSymbol node) : base(node)
        {
            IsNeedWhileIRGeneration = true;
        }



        // [0] : Ident [UseVarNode]
        // [1] : Ident [UseVarNode]
        // [2] : ActualParam? (AstNonTerminal)
        public override SdtsNode Compile(CompileParameter param)
        {
            _params.Clear();
            ConnectedErrInfoList.Clear();

            var ident = Items[0].Compile(param) as UseIdentNode;
            var functionName = Items[1].Compile(param) as UseIdentNode;
            if (Items.Count > 2)
            {
                var result = Items[2].Compile(param) as ActualParamNode;
                Params.AddRange(result.ParamNodeList);
            }

            AJChecker.IsDefinedSymbol(this, ident.IdentToken);
            var matchedList = AJUtilities.GetFuncList(this, functionName.IdentToken);

            if (matchedList.Count() == 0) AJAlarmFactory.CreateMCL0014(MethodNameToken.Input);
            else CheckParams(matchedList);

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


        private IEnumerable<FuncDefNode> GetParamAllMatchedList(IEnumerable<FuncDefNode> funcListToFind)
        {
            List<FuncDefNode> result = new List<FuncDefNode>();

            foreach (var func in funcListToFind)
            {
                if (GetMatchedTypeCount(func.ParamVarList) != func.ParamVarList.Count) continue;

                result.Add(func);
            }

            return result;
        }


        private Tuple<FuncDefNode, int> GetTopCandidate(IEnumerable<FuncDefNode> funcListToFind)
        {
            FuncDefNode result = null;
            int topMatchedIndex = -1;

            foreach (var func in funcListToFind)
            {
                var matchedIndex = GetMatchedTypeCount(func.ParamVarList);

                if (topMatchedIndex > matchedIndex) result = func;
            }

            return new Tuple<FuncDefNode, int>(result, topMatchedIndex);
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


        private void CheckParams(IEnumerable<FuncDefNode> matchedFuncList)
        {
            var funcList = GetParamCountMatchedList(matchedFuncList);
            if (funcList.Count() == 0)    // param count is not equal
            {
                AJAlarmFactory.CreateMCL0015(Params.Count(), MethodNameToken.Input);
                return;
            }

            var funcList2 = GetParamAllMatchedList(funcList);
            if (funcList2.Count() == 0)    // param count is equal but param type is not fit
            {
                var result = GetTopCandidate(funcList);
                var funcDefine = result.Item1;
                var paramIndex = result.Item2;

                AJAlarmFactory.CreateMCL0016(funcDefine.ToDefineString(false, true), 
                                                               funcDefine.ParamVarList[paramIndex].Name);
                return;
            }
            else
                Func = funcList2.First();
        }


        public override IRExpression To()
        {
            IRCallExpr result = new IRCallExpr();

            result.Function = Func.To() as IRFunction;

            return result;
        }

        public override IRExpression To(IRExpression from)
        {
            throw new NotImplementedException();
        }

        private List<IExprBuildNode> _params = new List<IExprBuildNode>();
    }
}