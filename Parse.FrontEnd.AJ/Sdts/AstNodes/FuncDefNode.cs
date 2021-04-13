using Parse.FrontEnd.Ast;
using Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.FrontEnd.AJ.Sdts.Datas.Variables;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public class FuncDefNode : AJNode, IHasSymbol, IHasVarInfos
    {
        public FuncHeadNode FuncHead { get; private set; }
        public CompoundStNode CompoundSt { get; private set; }

        public IEnumerable<VariableMiniC> VarList => FuncData?.ParamVarList;
        public IEnumerable<ISymbolData> SymbolList => VarList;

        public FuncDefData FuncData { get; private set; }

        public FuncDefNode(AstSymbol node) : base(node)
        {
            IsNeedWhileIRGeneration = true;
        }


        /**************************************************/
        /// <summary>
        /// It starts semantic analysis for function defenition.     <br/>
        /// 함수 정의에 대한 의미분석을 시작합니다.               <br/>
        /// </summary>
        /// <remark>
        /// [0] : Accessor (AccesserNode)                                <br/>
        /// [1] : FuncHead (FuncHeadNode)                             <br/>
        /// [2] : CompoundSt (CompoundStNode)                     <br/>
        /// </remark>
        /// <param name="param"></param>
        /// <returns></returns>
        /**************************************************/
        public override SdtsNode Build(SdtsParams param)
        {
            BlockLevel = ParentBlockLevel;

            // it needs to clone an param
            var newParam = param.Clone();
            var classDefNode = GetParent(typeof(ClassDefNode)) as ClassDefNode;

            // build FuncHead node
            FuncHead = Items[0].Build(newParam) as FuncHeadNode;

            FuncData = new FuncDefData(BlockLevel,
                                                    0,
                                                    false,
                                                    classDefNode.ClassData,
                                                    Access.Private,
                                                    FuncHead.ReturnType.MiniCTypeInfo,
                                                    FuncHead.NameToken,
                                                    FuncHead.ParamVarList.ToVarDataList,
                                                    CompoundSt);

            FuncData.ReferenceTable.Add(this);

            // build CompoundSt node
            CompoundSt = Items[1].Build(newParam) as CompoundStNode;
            FuncData.Statement = CompoundSt;

            return this;
        }
    }
}
