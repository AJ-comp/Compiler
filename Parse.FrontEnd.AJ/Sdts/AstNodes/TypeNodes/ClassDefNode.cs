using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.FrontEnd.Ast;
using System.Linq;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes
{
    public partial class ClassDefNode : TypeDefNode
    {
        public override string FullName
        {
            get
            {
                // if inner type def is support then this logic has to be changed.
                return RootNode.Namespace.FullName + "." + Name;
            }
        }

        public ClassDefNode(AstSymbol node) : base(node)
        {
        }


        /**************************************************/
        /// <summary>
        /// <para>Start semantic analysis for class.</para>
        /// <para>class에 대한 의미분석을 시작합니다.</para>
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        /// <remarks>
        /// format summary                                              <br/>
        /// [0] : accesser?                                                 <br/>
        /// [1] : Ident                                                        <br/>
        /// [2:n] : (accesser? (field | property | func))*       <br/>
        /// </remarks>
        /**************************************************/
        public override SdtsNode Compile(CompileParameter param)
        {
            base.Compile(param);

            if (param.Option == CompileOption.CheckAmbiguous) CheckForDuplication(param);
            else if (param.Option == CompileOption.CheckTypeDefine) CompileForTypeDefine(param);
            else if (param.Option == CompileOption.CheckMemberDeclaration) CompileForMember(param);
            else if (param.Option == CompileOption.Logic) CompileForLogic(param);

            //            DBContext.Instance.Insert(this);

            return this;
        }

        private void CompileForTypeDefine(CompileParameter param)
        {
            AccessType = Access.Private;

            if (Items[_memberOffset] is AccesserNode)
            {
                var accesserNode = Items[_memberOffset++].Compile(param) as AccesserNode;
                AccessType = accesserNode.AccessState;
            }

            var nameNode = Items[_memberOffset++].Compile(param) as DefNameNode;
            NameToken = nameNode.Token;
        }

        private void CompileForMember(CompileParameter param)
        {
            // field or property or function
            while (true)
            {
                if (Items.Count <= _memberOffset) break;

                Access accessType = Access.Private;
                if (Items[_memberOffset] is AccesserNode)
                {
                    var accesserNode = Items[_memberOffset++].Compile(param) as AccesserNode;
                    accessType = accesserNode.AccessState;
                }

                if (Items[_memberOffset] is DeclareVarStNode) CompileForVariableDclsNode(param.CloneForNewBlock(), _memberOffset, accessType);
                else if (Items[_memberOffset] is FuncDefNode) CompileForFuncDefNode(param.CloneForNewBlock(), _memberOffset, accessType);

                _memberOffset++;
            }

            // 현재 문법으로는 사용자 단에서 명시적으로 생성자와 소멸자를 생성할 수 없으므로
            // 여기서 디폴트 생성자 소멸자를 무조건 생성한다
            CreateDefaultCreator(param.CloneForNewBlock());
            CreateDefaultDestructor(param.CloneForNewBlock());

            References.Add(this);
        }


        private void CompileForLogic(CompileParameter param)
        {
            foreach (var func in AllFuncs.Where(x => x.StubCode == false)) func.Compile(param);
        }


        private void CheckForDuplication(CompileParameter param)
        {
            foreach(var defType in RootNode.DefTypes)
            {

            }
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
        private void CompileForVariableDclsNode(CompileParameter param, int offset, Access accessType)
        {
            param.Offset = Fields == null ? 0 : Fields.Count();
            // children node is parsing only variable elements so it doesn't need to clone an param
            var varDclsNode = Items[offset].Compile(param) as DeclareVarStNode;

            foreach (var varData in varDclsNode.VarList)
            {
                if (IsSameName(varData.NameToken)) continue;
                if (IsDuplicated(varData.NameToken)) continue;

                varData.AccessType = accessType;
                Fields.Add(varData);
            }
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
        private void CompileForFuncDefNode(CompileParameter param, int offset, Access accessType)
        {
            param.Offset = AllFuncs == null ? 0 : AllFuncs.Count();

            var node = Items[offset].Compile(param) as FuncDefNode;

            node.AccessType = accessType;
            if (IsSameName(node.NameToken)) return;
            if (IsDuplicated(node.NameToken)) return;
        }


        private void CreateDefaultCreator(CompileParameter param)
        {
            param.Offset = AllFuncs == null ? 0 : AllFuncs.Count();
            AJType returnTypeInfo = new AJPreDefType(AJDataType.Void);

            var func = new FuncDefNode(FuncType.Creator, Access.Public, param.BlockLevel, param.Offset, RootNode, returnTypeInfo);
            func.NameToken = TokenData.CreateStubToken(AJGrammar.Ident, Name);

            AutoGeneratedFuncs.Add(func);
        }


        private void CreateDefaultDestructor(CompileParameter param)
        {
            param.Offset = AllFuncs == null ? 0 : AllFuncs.Count();
            AJType returnTypeInfo = new AJPreDefType(AJDataType.Void);

            var func = new FuncDefNode(FuncType.Destructor, Access.Public, param.BlockLevel, param.Offset, RootNode, returnTypeInfo);
            func.NameToken = TokenData.CreateStubToken(AJGrammar.Ident, Name);

            AutoGeneratedFuncs.Add(func);
        }


        private int _memberOffset = 0;
    }
}