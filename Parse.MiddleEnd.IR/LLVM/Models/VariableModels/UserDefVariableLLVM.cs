using Parse.MiddleEnd.IR.Datas;
using Parse.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Parse.MiddleEnd.IR.LLVM.Models.VariableModels
{
    [DebuggerDisplay("{DebuggerDisplay, nq")]
    public class UserDefVariableLLVM : VariableLLVM
    {
        public override StdType TypeKind => StdType.Struct;
        public string TypeName { get; }
        public uint BiggestMemberSize { get; }

        public UserDefVariableLLVM(IRDeclareStructTypeVar var, bool isGlobal) : base(var, isGlobal)
        {
            TypeName = var.TypeName;
            BiggestMemberSize = var.BiggestMemberSize;
        }

        public UserDefVariableLLVM(int offset, uint pointerLevel) : base(offset, pointerLevel)
        {
        }

        public override string DebuggerDisplay => LLVMConverter.ToInstructionName(this);
    }
}
