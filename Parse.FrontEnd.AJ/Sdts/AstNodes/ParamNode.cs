using Parse.FrontEnd.Ast;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.FrontEnd.AJ.Sdts.Datas.Variables;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public class ParamNode : AJNode
    {
        public VariableTypeNode DataTypeNode { get; private set; }
        public DeclareVarNode VariableNode { get; private set; }
        public int Offset { get; private set; }
        public VariableMiniC ToVarData => _varData;

        public ParamNode(AstSymbol node) : base(node)
        {
        }



        // format summary
        // [0] : VariableTypeNode [DclSpec]
        // [1] : VariableNode [Variable]
        public override SdtsNode Build(SdtsParams param)
        {
            _varData = null;
            BlockLevel = param.BlockLevel;
            Offset = param.Offset;

            SymbolTable = (param as AJSdtsParams).SymbolTable;

            DataTypeNode = Items[0].Build(param) as VariableTypeNode;
            VariableNode = Items[1].Build(param) as DeclareVarNode;

            // If it is virtual token it has not to add to symbol table.
            if (VariableNode.NameToken.IsVirtual) return this;

            if (!AJChecker.CanAddVarData(this, ToVarData)) return this;

            // save varData to SymbolTable
            (param as AJSdtsParams).SymbolTable
                                                      .VarTable
                                                      .CreateNewBlock(ToVarData, new ReferenceInfo(this));

            _varData = AJCreator.CreateVarData(Access.Private,
                                                                        DataTypeNode.MiniCTypeInfo,
                                                                        VariableNode.NameToken,
                                                                        null,
                                                                        VariableNode.DimensionToken,
                                                                        BlockLevel,
                                                                        Offset,
                                                                        null);

            return this;
        }



        private VariableMiniC _varData;
    }
}
