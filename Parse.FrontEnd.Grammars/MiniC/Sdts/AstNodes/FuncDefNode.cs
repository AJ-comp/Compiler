using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.StatementNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;
using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes
{
    public class FuncDefNode : MiniCNode
    {
        public FuncHeadNode FuncHead { get; private set; }
        public int Offset { get; private set; }

        public MiniCFuncData FuncData
            => new MiniCFuncData(FuncHead.ReturnType.MiniCTypeInfo,
                                               FuncHead.NameToken,
                                               0,
                                               FuncHead.ParamVarList.ToVarDataList);


        public FuncDefNode(AstSymbol node) : base(node)
        {
        }


        // [0] : FuncHead (AstNonTerminal)
        // [1] : CompoundSt (AstNonTerminal)
        public override SdtsNode Build(SdtsParams param)
        {
            // it needs to clone an param
            var newParam = CreateParamForNewBlock(param);

            // build FuncHead node
            FuncHead = Items[0].Build(newParam) as FuncHeadNode;

            // build CompoundSt node
            var token = Items[1].Build(newParam) as CompoundStNode;

            return this;
        }
    }
}
