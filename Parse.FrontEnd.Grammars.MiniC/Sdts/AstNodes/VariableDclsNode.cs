using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;
using System.Collections.Generic;
using static Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables.VariableMiniC;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes
{
    public class VariableDclsNode : MiniCNode
    {
        public VariableDclsNode(AstSymbol node) : base(node)
        {
        }

        public VariableTypeNode VarType { get; private set; }
        public IReadOnlyList<InitDeclaratorNode> InitDeclarators => _initDeclarators;

        public int BlockLevel { get; private set; }
        public int Offset { get; private set; }
        public VarProperty VariableProperty { get; internal set; }


        // format summary
        // [0] : VariableTypeNode [DclSpec]
        // [1] : InitDeclaratorNode* [InitDeclarator]
        public override SdtsNode Build(SdtsParams param)
        {
            // build VariableTypeNode
            VarType = Items[0].Build(param) as VariableTypeNode;

            // build InitDeclaratorNodes
            for (int i = 1; i < Items.Count; i++)
            {
                var initDeclarator = Items[i].Build(param) as InitDeclaratorNode;
                if (initDeclarator.ConnectedErrInfoList.Count > 0) continue;

                _initDeclarators.Add(initDeclarator);

                BlockLevel = param.BlockLevel;
                Offset = param.Offset++;

                // convert to VarData to save to SymbolTable
                var varData = MiniCCreator.CreateVarData(VarType.MiniCTypeInfo,
                                                                               initDeclarator.NameToken,
                                                                               initDeclarator.LevelToken,
                                                                               initDeclarator.DimensionToken,
                                                                               BlockLevel,
                                                                               Offset,
                                                                               VarProperty.Normal,
                                                                               initDeclarator.RightValue);

                // save to SymbolTable
                (param as MiniCSdtsParams).SymbolTable
                                                          .AddVarData(varData, new ReferenceInfo(this, varData.ValueConstant));
            }

            return this;
        }


        private List<InitDeclaratorNode> _initDeclarators = new List<InitDeclaratorNode>();
    }
}
