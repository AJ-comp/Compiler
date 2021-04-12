using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Sdts.Datas;
using Parse.FrontEnd.MiniC.Sdts.Datas.Variables;
using Parse.MiddleEnd.IR.Datas;
using System.Collections.Generic;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes
{
    public class VariableDclListNode : MiniCNode, IHasVarInfos
    {
        public VariableDclListNode(AstSymbol node) : base(node)
        {
        }

        public VariableTypeNode VarType { get; private set; }
        public IEnumerable<VariableMiniC> VarList => _varDatas;
        public int Offset { get; private set; }
        public VarProperty VariableProperty { get; internal set; }


        // format summary
        // [0] : VariableTypeNode [DclSpec]
        // [1] : InitDeclaratorNode* [InitDeclarator]
        public override SdtsNode Build(SdtsParams param)
        {
            BlockLevel = ParentBlockLevel;

            _varDatas.Clear();
            ConnectedErrInfoList.Clear();

            int startOffset = 0;
            Access accessState = Access.Private;
            if(Items[0] is AccesserNode)
            {
                var accesserNode = Items[0].Build(param) as AccesserNode;
                accessState = accesserNode.AccessState;
                startOffset = 1;
            }

            // build VariableTypeNode
            VarType = Items[startOffset].Build(param) as VariableTypeNode;
            //if (VarType.DataType == MiniCDataType.Void)
            //{
            //    ConnectedErrInfoList.Add(new MeaningErrInfo(nameof(AlarmCodes.MCL0020), AlarmCodes.MCL0020));
            //    return this;
            //}

            // build InitDeclaratorNodes
            for (int i = startOffset + 1; i < Items.Count; i++)
            {
                var initDeclarator = Items[i].Build(param) as InitDeclaratorNode;
                if (initDeclarator.ConnectedErrInfoList.Count > 0) continue;
                if (initDeclarator.NameToken.IsVirtual) continue;

                Offset = param.Offset++;

                // convert to VarData to save to SymbolTable
                var varData = MiniCCreator.CreateVarData(accessState,
                                                                               VarType.MiniCTypeInfo,
                                                                               initDeclarator.NameToken,
                                                                               initDeclarator.LevelToken,
                                                                               initDeclarator.DimensionToken,
                                                                               BlockLevel,
                                                                               Offset,
                                                                               initDeclarator.Right);

                if (!MiniCChecker.CanAddVarData(this, varData)) continue;
                _varDatas.Add(varData);
                varData.ReferenceTable.Add(this);
            }

            return this;
        }

        private List<VariableMiniC> _varDatas = new List<VariableMiniC>();
    }
}
