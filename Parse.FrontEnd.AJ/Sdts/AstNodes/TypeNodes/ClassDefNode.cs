using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.FrontEnd.Ast;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes
{
    public partial class ClassDefNode : UserDefTypeNode
    {
        public ClassDefNode(AstSymbol node) : base(node)
        {
            BlockLevel = 2;
        }


        /**************************************************/
        /// <summary>
        /// Start semantic analysis for class.                  <br/>
        /// class에 대한 의미분석을 시작합니다.                <br/>
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        /// <remarks>
        /// [0] : accesser?                                                 <br/>
        /// [1] : Ident                                                        <br/>
        /// [2:n] : (accesser? (field | property | func))*       <br/>
        /// </remarks>
        /**************************************************/
        public override SdtsNode Compile(CompileParameter param)
        {
            AccessType = Access.Private;
            var newParam = param.Clone();

            int offset = 0;
            if (Items[offset] is AccesserNode)
            {
                var accesserNode = Items[offset++].Compile(param) as AccesserNode;
                if (accesserNode.AccessState == Access.Public) AccessType = Access.Public;
            }

            var identNode = Items[offset++].Compile(param) as TerminalNode;
            DataTypeToken = identNode.Token;

            // field or property or function
            while (true)
            {
                if (Items.Count <= offset) break;

                Access accessType = Access.Private;
                if (Items[offset] is AccesserNode)
                {
                    var accesserNode = Items[offset++].Compile(param) as AccesserNode;
                    if (accesserNode.AccessState == Access.Public) accessType = Access.Public;
                }

                if (Items[offset] is DeclareVarStNode) BuildForVariableDclsNode(newParam, offset++, accessType);
                else if (Items[offset] is FuncDefNode) BuildForFuncDefNode(newParam, offset++, accessType);
            }

            // 현재 문법으로는 사용자 단에서 명시적으로 생성자와 소멸자를 생성할 수 없으므로
            // 여기서 디폴트 생성자 소멸자를 무조건 생성한다
            CreateDefaultCreator(newParam);
            CreateDefaultDestructor(newParam);

            References.Add(this);
            DBContext.Instance.Insert(this);

            return this;
        }


        /**************************************************/
        /// <summary>
        /// <para>Start semantic analysis for class member variable.</para>
        /// <para>class 멤버 변수에 대한 의미분석을 시작합니다.</para>
        /// </summary>
        /// <param name="param"></param>
        /// <param name="offset"></param>
        /// <param name="accessType"></param>
        /**************************************************/
        private void BuildForVariableDclsNode(CompileParameter param, int offset, Access accessType)
        {
            param.Offset = (Fields == null) ? 0 : Fields.Count();
            // children node is parsing only variable elements so it doesn't need to clone an param
            var varDclsNode = Items[offset].Compile(param) as DeclareVarStNode;

            foreach (var varData in varDclsNode.VarList) varData.AccessType = accessType;
        }


        /**************************************************/
        /// <summary>
        /// <para>Start semantic analysis for class member function.</para>
        /// <para>class 멤버 함수에 대한 의미분석을 시작합니다.</para>
        /// </summary>
        /// <param name="param"></param>
        /// <param name="offset"></param>
        /// <param name="accessType"></param>
        /**************************************************/
        private void BuildForFuncDefNode(CompileParameter param, int offset, Access accessType)
        {
            param.Offset = 0;

            var node = Items[offset].Compile(param) as FuncDefNode;
            node.AccessType = accessType;
        }


        private void CreateDefaultCreator(CompileParameter param)
        {
            AJTypeInfo returnTypeInfo = new AJTypeInfo(AJDataType.Void, TokenData.CreateVirtualToken(AJGrammar.Void));

            AllFuncs.Add(new FuncDefNode(FuncType.Creator, Access.Public, param.BlockLevel, param.Offset, returnTypeInfo));
        }


        private void CreateDefaultDestructor(CompileParameter param)
        {
            AJTypeInfo returnTypeInfo = new AJTypeInfo(AJDataType.Void, TokenData.CreateVirtualToken(AJGrammar.Void));

            AllFuncs.Add(new FuncDefNode(FuncType.Destructor, Access.Public, param.BlockLevel, param.Offset, returnTypeInfo));
        }
    }
}
