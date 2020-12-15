using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.StatementNodes;
using Parse.FrontEnd.MiniC.Sdts.Datas;
using Parse.FrontEnd.MiniC.Sdts.Datas.Variables;
using System.Collections.Generic;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes
{
    public class FuncDefNode : MiniCNode, IHasVarInfos
    {
        public FuncHeadNode FuncHead { get; private set; }
        public CompoundStNode CompoundSt { get; private set; }

        public IEnumerable<VariableMiniC> VarList => FuncData?.ParamVars;

        public FuncData FuncData { get; private set; }

        public FuncDefNode(AstSymbol node) : base(node)
        {
            IsNeedWhileIRGeneration = true;
        }


        // [0] : Accessor (AccesserNode)
        // [0] : FuncHead (FuncHeadNode)
        // [1] : CompoundSt (CompoundStNode)
        public override SdtsNode Build(SdtsParams param)
        {
            // it needs to clone an param
            var newParam = CreateParamForNewBlock(param);
            SymbolTable = newParam.SymbolTable;

            var accesserNode = Items[0].Build(newParam) as AccesserNode;
            var accessState = accesserNode.AccessState;
            
            // build FuncHead node
            FuncHead = Items[1].Build(newParam) as FuncHeadNode;

            // build CompoundSt node
            CompoundSt = Items[2].Build(newParam) as CompoundStNode;

            FuncData = new FuncData(accessState, 
                                                    FuncHead.ReturnType.MiniCTypeInfo, 
                                                    FuncHead.NameToken, 
                                                    0, 
                                                    FuncHead.ParamVarList.ToVarDataList);

            FuncData.ReferenceTable.Add(this);

            return this;
        }
    }
}
