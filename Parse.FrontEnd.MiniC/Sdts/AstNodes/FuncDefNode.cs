using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.StatementNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables;
using Parse.MiddleEnd.IR.Datas;
using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes
{
    public class FuncDefNode : MiniCNode
    {
        public FuncHeadNode FuncHead { get; private set; }
        public CompoundStNode CompoundSt { get; private set; }

        public int Offset { get; private set; }

        public MiniCFuncData FuncData
            => new MiniCFuncData(FuncHead.ReturnType.MiniCTypeInfo,
                                               FuncHead.NameToken,
                                               0,
                                               FuncHead.ParamVarList.ToVarDataList);


        public FuncDefNode(AstSymbol node) : base(node)
        {
            IsNeedWhileIRGeneration = true;
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
            CompoundSt = Items[1].Build(newParam) as CompoundStNode;

            return this;
        }

        public IRFuncData ToIRFuncData()
        {
            List<IRVar> paramVars = new List<IRVar>();

            foreach (var varTable in SymbolTable.AllVarTable)
            {
                foreach (var varRecord in varTable)
                    if (varRecord.DefineField.VariableProperty == VarProperty.Param) paramVars.Add(varRecord.DefineField);
            }

            FuncHeadNode funcHead = FuncHead;

            return new IRFuncData(paramVars,
                                                funcHead.ReturnType.Const,
                                                IRConverter.ToIRReturnType(funcHead.ReturnType.DataType),
                                                funcHead.Name,
                                                0
                                                );
        }
    }
}
