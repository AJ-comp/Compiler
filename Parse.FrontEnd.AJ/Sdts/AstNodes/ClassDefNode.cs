using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.FrontEnd.Ast;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public class ClassDefNode : AJNode, IHasSymbol, IClassExpression
    {
        public IEnumerable<ISymbolData> SymbolList => ClassData?.SymbolList;


        // for IClassExpression interface
        public IEnumerable<IDeclareVarExpression> VarList => ClassData?.Fields;
        public IEnumerable<IFunctionExpression> FuncList => ClassData?.Funcs;
        public Access AccessType => ClassData.AccessType;
        public string Name => ClassData.Name;


        /**************************************************/
        /// <summary>
        /// It gets member variable list.
        /// 멤버 변수 리스트를 가져옵니다.
        /// </summary>
        /**************************************************/
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


        /**************************************************/
        /// <summary>
        /// It gets static variable list.
        /// static 변수 리스트를 가져옵니다.
        /// </summary>
        /**************************************************/
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


        /**************************************************/
        /// <summary>
        /// It gets member function list.
        /// 멤버 함수 리스트를 가져옵니다.
        /// </summary>
        /**************************************************/
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


        /**************************************************/
        /// <summary>
        /// It gets static function list.
        /// static 함수 리스트를 가져옵니다.
        /// </summary>
        /**************************************************/
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


        /**************************************************/
        /// <summary>
        /// It starts semantic analysis for class.
        /// class에 대한 의미분석을 시작합니다.
        /// [0] : accesser?
        /// [1] : Ident
        /// [2:n] : (accesser? (field | property | func))*
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        /**************************************************/
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


        /**************************************************/
        /// <summary>
        /// It starts semantic analysis for class member variable.
        /// class 멤버 변수에 대한 의미분석을 시작합니다.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="offset"></param>
        /// <param name="accessType"></param>
        /// <param name="symbolList"></param>
        /**************************************************/
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


        /**************************************************/
        /// <summary>
        /// It starts semantic analysis for class member function.
        /// class 멤버 함수에 대한 의미분석을 시작합니다.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="offset"></param>
        /// <param name="accessType"></param>
        /// <param name="symbolList"></param>
        /**************************************************/
        private void BuildForFuncDefNode(SdtsParams param, int offset, Access accessType, HashSet<ISymbolData> symbolList)
        {
            param.Offset = 0;

            var node = Items[offset].Build(param) as FuncDefNode;
            node.FuncData.AccessType = accessType;
            symbolList.Add(node.FuncData);
        }
    }
}
