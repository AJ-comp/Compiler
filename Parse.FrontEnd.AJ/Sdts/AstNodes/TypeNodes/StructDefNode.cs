using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes;
using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class StructDefNode : TypeDefNode
    {
        public StructDefNode(AstSymbol node) : base(node)
        {
        }

        public override AJDataType DefType => AJDataType.Struct;
        public override uint Size
        {
            get
            {
                foreach (var predefType in PreDefTypeList)
                {
                    if (FullName == predefType.DefineFullName) return predefType.Size;
                }

                uint size = 0;
                foreach (var member in _fields)
                    size += member.Type.Size;

                return size;
            }
        }
        public override IEnumerable<VariableAJ> AllFields => throw new NotImplementedException();


        // [0] : Ident [TerminalNode]
        // [1] : declaration
        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            base.CompileLogic(param);

            if (param.Option == CompileOption.CheckTypeDefine) CompileForTypeDefine(param);
            else if (param.Option == CompileOption.CheckMemberDeclaration) CompileForMember(param);
            else if (param.Option == CompileOption.Logic) CompileForLogic(param);

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
        }


        private void CompileForLogic(CompileParameter param)
        {
            foreach (var func in AllFuncs.Where(x => x.StubCode == false)) func.Compile(param);
        }



        private int _memberOffset = 0;

        private string GetDebuggerDisplay()
            => $"{AccessType} struct {Name} (field: {_fields.Count()}, func: {AllFuncs.Count()}) [{GetType().Name}]";
    }
}
