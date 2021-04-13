using Parse.FrontEnd.Ast;
using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Interfaces;
using Parse.Types;
using Parse.Types.ConstantTypes;
using Parse.Types.VarTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes
{
    public class CallNode : ExprNode, ICallExpression
    {
        public FuncDefData FuncData { get; private set; }
        public IFunctionExpression CallFuncDef => FuncData;

        public TokenData MethodNameToken { get; private set; }
        public IEnumerable<IExprExpression> Params => _params;
        public IReadOnlyList<IConstant> ParamsTypeList
        {
            get
            {
                List<IConstant> result = new List<IConstant>();

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

            AJChecker.IsDefinedSymbol(this, ident.IdentToken);
            var matchedList = AJUtilities.GetFuncDataList(this, functionName.IdentToken);

            if (matchedList.Count() == 0) AddMCL0014Exception();
            else CheckParams(matchedList);

            return this;
        }


        private IEnumerable<FuncDefData> GetParamCountMatchedList(IEnumerable<FuncDefData> funcListToFind)
        {
            List<FuncDefData> result = new List<FuncDefData>();

            foreach (var func in funcListToFind)
            {
                if (func.ParamVarList.Count() != Params.Count()) continue;

                result.Add(func);
            }

            return result;
        }


        private IEnumerable<FuncDefData> GetParamAllMatchedList(IEnumerable<FuncDefData> funcListToFind)
        {
            List<FuncDefData> result = new List<FuncDefData>();

            foreach (var func in funcListToFind)
            {
                if (GetMatchedTypeCount(func.ParamVarList, ParamsTypeList) != func.ParamVarList.Count) continue;

                result.Add(func);
            }

            return result;
        }


        private Tuple<FuncDefData, int> GetTopCandidate(IEnumerable<FuncDefData> funcListToFind)
        {
            FuncDefData result = null;
            int topMatchedIndex = -1;

            foreach (var func in funcListToFind)
            {
                var matchedIndex = GetMatchedTypeCount(func.ParamVarList, ParamsTypeList);

                if (topMatchedIndex > matchedIndex) result = func;
            }

            return new Tuple<FuncDefData, int>(result, topMatchedIndex);
        }


        private int GetMatchedTypeCount(IEnumerable<IDeclareVarExpression> srcList, IReadOnlyList<IConstant> targetList)
        {
            if (srcList.Count() != targetList.Count) return 0;

            int result = 0;
            for (int i = 0; i < srcList.Count(); i++)
            {
                try
                {
                    IDeclareVarExpression src = srcList.ElementAt(i);
                    IValue target = targetList.ElementAt(i);

                    if (src is IVariable) (src as IVariable).Assign(targetList[i]);
                    else if (src.TypeKind != target.TypeKind) break;

                    result++;
                }
                catch
                {
                    break;
                }
            }

            return result;
        }


        private void CheckParams(IEnumerable<FuncDefData> matchedFuncList)
        {
            var funcList = GetParamCountMatchedList(matchedFuncList);
            if (funcList.Count() == 0)    // param count is not equal
            {
                AddMCL0015Exception(Params.Count());
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

        private void AddMCL0016Exception(FuncDefData funcDefine, int paramIndex)
        {
            ConnectedErrInfoList.Add
                (
                    new MeaningErrInfo(MethodNameToken,
                                                    nameof(AlarmCodes.MCL0016),
                                                    string.Format(AlarmCodes.MCL0016,
                                                                        funcDefine.ToDefineString(false, true),
                                                                        funcDefine.ParamVarList[paramIndex].Name))
                );
        }


        private List<IExprExpression> _params = new List<IExprExpression>();
    }
}
