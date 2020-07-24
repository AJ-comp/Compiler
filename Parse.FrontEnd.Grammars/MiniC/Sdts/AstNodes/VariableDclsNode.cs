using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;
using System.Collections.Generic;

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
        public EtcInfo Etc { get; internal set; }


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
                MiniCVarData varData = new MiniCVarData(VarType.MiniCTypeInfo,
                                                                                initDeclarator.NameToken,
                                                                                initDeclarator.LevelToken,
                                                                                initDeclarator.DimensionToken,
                                                                                BlockLevel,
                                                                                Offset,
                                                                                EtcInfo.Normal,
                                                                                initDeclarator.Value);

                // save to SymbolTable
                (param as MiniCSdtsParams).SymbolTable.VarList.Add(varData);
            }

            return this;
        }

        private List<InitDeclaratorNode> _initDeclarators = new List<InitDeclaratorNode>();
    }
}
