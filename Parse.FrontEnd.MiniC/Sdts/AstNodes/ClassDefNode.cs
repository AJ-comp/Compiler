using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Sdts.Datas;
using Parse.FrontEnd.MiniC.Sdts.Datas.Variables;
using System.Collections.Generic;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes
{
    public class ClassDefNode : MiniCNode, IHasVarInfos, IHasFuncInfos
    {
        public IEnumerable<VariableMiniC> VarList => ClassData?.Fields;
        public IEnumerable<FuncData> FuncList => ClassData?.Funcs;

        public ClassData ClassData { get; private set; }

        public ClassDefNode(AstSymbol node) : base(node)
        {
        }

        // [0] : accesser?
        // [1] : Ident
        // [2:n] : Dcl? (AstNonTerminal)
        // [n+1:y] : FuncDef? (AstNonTerminal)
        public override SdtsNode Build(SdtsParams param)
        {
            var accessState = Access.Private;
            TokenData classToken = null;
            var newParam = param.CloneForNewBlock();

            HashSet<VariableMiniC> varList = new HashSet<VariableMiniC>();
            HashSet<FuncData> funcDataList = new HashSet<FuncData>();

            foreach (var item in Items)
            {
                var minicNode = item as MiniCNode;

                // Accesser
                if (minicNode is AccesserNode)
                {
                    var accesserNode = minicNode.Build(param) as AccesserNode;
                    if (accesserNode.AccessState == Access.Public) accessState = Access.Public;
                }
                // Ident
                else if (minicNode is TerminalNode)
                {
                    var identNode = minicNode.Build(param) as TerminalNode;
                    classToken = identNode.Token;
                }
                // Member variable
                else if (minicNode is VariableDclsNode)
                {
                    // children node is parsing only variable elements so it doesn't need to clone an param
                    var varDclsNode = minicNode.Build(newParam) as VariableDclsNode;
                    foreach (var varData in varDclsNode.VarList) varList.Add(varData);
                }
                // Member function
                else if (minicNode is FuncDefNode)
                {
                    newParam.Offset = 0;

                    var node = minicNode.Build(newParam) as FuncDefNode;
                    funcDataList.Add(node.FuncData);
                }
            }

            ClassData = new ClassData(accessState, classToken, varList, funcDataList);
            ClassData.ReferenceTable.Add(this);

            return this;
        }
    }
}
