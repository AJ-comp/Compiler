using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables;
using Parse.FrontEnd.Grammars.Properties;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes
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
                return MiniCCreator.CreateVarData(DataTypeNode.MiniCTypeInfo,
                                                                    VariableNode.NameToken,
                                                                    null,
                                                                    VariableNode.DimensionToken,
                                                                    BlockLevel,
                                                                    Offset,
                                                                    VariableMiniC.VarProperty.Param);
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

            DataTypeNode = Items[0].Build(param) as VariableTypeNode;
            VariableNode = Items[1].Build(param) as DeclareVarNode;

            // If it is virtual token it has not to add to symbol table.
            if (VariableNode.NameToken.IsVirtual) return this;

            // save varData to SymbolTable
            bool result = (param as MiniCSdtsParams).SymbolTable
                                                                         .VarTable
                                                                         .CreateNewBlock(ToVarData, new ReferenceInfo(this));

            // duplicated declaration
            if (!result) MiniCUtilities.AddDuplicatedError(this, VariableNode.NameToken);

            return this;
        }
    }
}
