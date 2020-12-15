using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Properties;
using Parse.FrontEnd.MiniC.Sdts.Datas;
using Parse.FrontEnd.MiniC.Sdts.Datas.Variables;
using Parse.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes
{
    public class CallNode : ExprNode
    {
        public FuncData FuncData { get; private set; }

        public TokenData MethodNameToken { get; private set; }
        public IReadOnlyList<ExprNode> Params => _params;
        public IReadOnlyList<IValue> ParamsTypeList
        {
            get
            {
                List<IValue> result = new List<IValue>();

                foreach (var param in Params) result.Add(param.Result);

                return result;
            }
        }

        public CallNode(AstSymbol node) : base(node)
        {
            IsNeedWhileIRGeneration = true;
        }



        // [0] : Ident [UseVarNode]
        // [1] : Ident [UseVarNode]
        // [2] : ActualParam? (AstNonTerminal)
        public override SdtsNode Build(SdtsParams param)
        {
            _params.Clear();
            ConnectedErrInfoList.Clear();

            var ident = Items[0].Build(param) as UseIdentNode;
            var functionName = Items[1].Build(param) as UseIdentNode;
            if (Items.Count > 2)
            {
                var result = Items[2].Build(param) as ActualParamNode;
                _params.AddRange(result.ParamNodeList);
            }

            MiniCChecker.IsDefinedVar(this, ident.IdentToken);
            var matchedList = MiniCUtilities.GetFuncDataList(this, functionName.IdentToken);

            if (matchedList.Count() == 0) AddMCL0014Exception();
            else CheckParams(matchedList);

            return this;
        }


        private IEnumerable<FuncData> GetParamCountMatchedList(IEnumerable<FuncData> funcListToFind)
        {
            List<FuncData> result = new List<FuncData>();

            foreach (var func in funcListToFind)
            {
                if (func.ParamVars.Count() != Params.Count) continue;

                result.Add(func);
            }

            return result;
        }


        private IEnumerable<FuncData> GetParamAllMatchedList(IEnumerable<FuncData> funcListToFind)
        {
            List<FuncData> result = new List<FuncData>();

            foreach (var func in funcListToFind)
            {
                if (GetMatchedTypeCount(func.ParamVars, ParamsTypeList) != func.ParamVars.Count) continue;

                result.Add(func);
            }

            return result;
        }


        private Tuple<FuncData, int> GetTopCandidate(IEnumerable<FuncData> funcListToFind)
        {
            FuncData result = null;
            int topMatchedIndex = -1;

            foreach (var func in funcListToFind)
            {
                var matchedIndex = GetMatchedTypeCount(func.ParamVars, ParamsTypeList);

                if (topMatchedIndex > matchedIndex) result = func;
            }

            return new Tuple<FuncData, int>(result, topMatchedIndex);
        }


        private int GetMatchedTypeCount(IEnumerable<VariableMiniC> src, IReadOnlyList<IValue> target)
        {
            if (src.Count() != target.Count) return 0;

            int result = 0;
            for (int i = 0; i < src.Count(); i++)
            {
                try
                {
                    src.ElementAt(i).Assign(target[i]);
                    result++;
                }
                catch
                {
                    break;
                }
            }

            return result;
        }


        private void CheckParams(IEnumerable<FuncData> matchedFuncList)
        {
            var funcList = GetParamCountMatchedList(matchedFuncList);
            if (funcList.Count() == 0)    // param count is not equal
            {
                AddMCL0015Exception(Params.Count);
                return;
            }

            var funcList2 = GetParamAllMatchedList(funcList);
            if (funcList2.Count() == 0)    // param count is equal but param type is not fit
            {
                var result = GetTopCandidate(funcList);
                AddMCL0016Exception(result.Item1, result.Item2);
                return;
            }
            else
                FuncData = funcList2.First();
        }



        private void AddMCL0014Exception()
        {
            ConnectedErrInfoList.Add
                (
                    new MeaningErrInfo(MethodNameToken,
                                                    nameof(AlarmCodes.MCL0014),
                                                    string.Format(AlarmCodes.MCL0014, MethodNameToken.Input))
                );
        }

        private void AddMCL0015Exception(int paramCount)
        {
            ConnectedErrInfoList.Add
                (
                    new MeaningErrInfo(MethodNameToken,
                                                    nameof(AlarmCodes.MCL0015),
                                                    string.Format(AlarmCodes.MCL0015, paramCount, MethodNameToken.Input))
                );
        }

        private void AddMCL0016Exception(FuncData funcDefine, int paramIndex)
        {
            ConnectedErrInfoList.Add
                (
                    new MeaningErrInfo(MethodNameToken,
                                                    nameof(AlarmCodes.MCL0016),
                                                    string.Format(AlarmCodes.MCL0016,
                                                                        funcDefine.ToDefineString(false, true),
                                                                        funcDefine.ParamVars[paramIndex].Name))
                );
        }


        private List<ExprNode> _params = new List<ExprNode>();
    }
}
