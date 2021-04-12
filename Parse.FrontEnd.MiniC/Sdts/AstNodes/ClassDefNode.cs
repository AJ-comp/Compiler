using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Sdts.Datas;
using Parse.MiddleEnd.IR.Datas;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes
{
    public class ClassDefNode : MiniCNode, IHasSymbol, IClassExpression
    {
        public IEnumerable<ISymbolData> SymbolList => ClassData?.SymbolList;


        // for interface
        public IEnumerable<IDeclareVarExpression> VarList => ClassData?.Fields;
        public IEnumerable<IFunctionExpression> FuncList => ClassData?.Funcs;
        public Access AccessType => ClassData.AccessType;
        public string Name => ClassData.Name;



        public IEnumerable<IDeclareVarExpression> MemberVarList
        {
            get
            {
                List<IDeclareVarExpression> result = new List<IDeclareVarExpression>();
                foreach (var varData in VarList)
                {
                    // if static is not
                    result.Add(varData);
                }

                return result;
            }
        }

        public IEnumerable<IDeclareVarExpression> StaticVarList
        {
            get
            {
                List<IDeclareVarExpression> result = new List<IDeclareVarExpression>();
                foreach (var varData in VarList)
                {
                    // add if static is
                    result.Add(varData);
                }

                return result;
            }
        }


        public IEnumerable<IFunctionExpression> MemberFuncList
        {
            get
            {
                List<IFunctionExpression> result = new List<IFunctionExpression>();
                foreach (var funcData in FuncList)
                {
                    // if static is not 
                    result.Add(funcData);
                }

                return result;
            }
        }

        public IEnumerable<IFunctionExpression> StaticFuncList
        {
            get
            {
                List<IFunctionExpression> result = new List<IFunctionExpression>();
                foreach (var funcData in FuncList)
                {
                    // if static
                    result.Add(funcData);
                }

                return result;
            }
        }

        public ClassDefData ClassData { get; private set; }

        public ClassDefNode(AstSymbol node) : base(node)
        {
            BlockLevel = 2;
        }

        // [0] : accesser?
        // [1] : Ident
        // [2:n] : (accesser? (field | property | func))*
        public override SdtsNode Build(SdtsParams param)
        {
            var accessState = Access.Private;
            var newParam = param.Clone();

            HashSet<ISymbolData> symbolList = new HashSet<ISymbolData>();

            int offset = 0;
            if (Items[offset] is AccesserNode)
            {
                var accesserNode = Items[offset++].Build(param) as AccesserNode;
                if (accesserNode.AccessState == Access.Public) accessState = Access.Public;
            }

            var identNode = Items[offset++].Build(param) as TerminalNode;
            var classToken = identNode.Token;

            ClassData = new ClassDefData(BlockLevel, accessState, classToken, symbolList);
            ClassData.ReferenceTable.Add(this);

            // field or property or function
            while (true)
            {
                if (Items.Count <= offset) break;

                Access accessType = Access.Private;
                if (Items[offset] is AccesserNode)
                {
                    var accesserNode = Items[offset++].Build(param) as AccesserNode;
                    if (accesserNode.AccessState == Access.Public) accessType = Access.Public;
                }

                if (Items[offset] is VariableDclListNode) BuildForVariableDclsNode(newParam, offset++, accessType, symbolList);
                else if (Items[offset] is FuncDefNode) BuildForFuncDefNode(newParam, offset++, accessType, symbolList);
            }

            return this;
        }


        private void BuildForVariableDclsNode(SdtsParams param, int offset, Access accessType, HashSet<ISymbolData> symbolList)
        {
            param.Offset = (VarList == null) ? 0 : VarList.Count();
            // children node is parsing only variable elements so it doesn't need to clone an param
            var varDclsNode = Items[offset].Build(param) as VariableDclListNode;

            foreach (var varData in varDclsNode.VarList)
            {
                varData.AccessType = accessType;
                varData.PartyName = Name;

                symbolList.Add(varData);
            }
        }


        private void BuildForFuncDefNode(SdtsParams param, int offset, Access accessType, HashSet<ISymbolData> symbolList)
        {
            param.Offset = 0;

            var node = Items[offset].Build(param) as FuncDefNode;
            node.FuncData.AccessType = accessType;
            symbolList.Add(node.FuncData);
        }
    }
}
