using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Sdts.Datas;
using Parse.FrontEnd.MiniC.Sdts.Datas.Variables;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes
{
    public class ParamNode : MiniCNode
    {
        public VariableTypeNode DataTypeNode { get; private set; }
        public DeclareVarNode VariableNode { get; private set; }
        public int BlockLevel { get; private set; }
        public int Offset { get; private set; }

        public VariableMiniC ToVarData
        {
            get
            {
                return MiniCCreator.CreateVarData(Access.Private,
                                                                    DataTypeNode.MiniCTypeInfo,
                                                                    VariableNode.NameToken,
                                                                    null,
                                                                    VariableNode.DimensionToken,
                                                                    BlockLevel,
                                                                    Offset,
                                                                    VarProperty.Param);
            }
        }

        public ParamNode(AstSymbol node) : base(node)
        {
        }



        // format summary
        // [0] : VariableTypeNode [DclSpec]
        // [1] : VariableNode [Variable]
        public override SdtsNode Build(SdtsParams param)
        {
            BlockLevel = param.BlockLevel;
            Offset = param.Offset;

            SymbolTable = (param as MiniCSdtsParams).SymbolTable;

            DataTypeNode = Items[0].Build(param) as VariableTypeNode;
            VariableNode = Items[1].Build(param) as DeclareVarNode;

            // If it is virtual token it has not to add to symbol table.
            if (VariableNode.NameToken.IsVirtual) return this;

            if (!MiniCChecker.CanAddVarData(this, ToVarData)) return this;

            // save varData to SymbolTable
            (param as MiniCSdtsParams).SymbolTable
                                                      .VarTable
                                                      .CreateNewBlock(ToVarData, new ReferenceInfo(this));

            return this;
        }
    }
}
