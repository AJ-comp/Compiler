using Parse.MiddleEnd.IR.LLVM;
using Parse.Types;
using System;
using System.Diagnostics;

namespace Parse.MiddleEnd.IR.Datas
{
    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public abstract class UseDefChainVar// : IUseDefChainable, ICanBePointerType
    {
        public IRDeclareVar VarInfo { get; }

        public abstract string Name { get; }

        public string TypeName
        {
            get
            {
                if (VarInfo == null) return string.Empty;

                if (VarInfo is IRDeclareStructTypeVar)
                {
                    var cVarInfo = VarInfo as IRDeclareStructTypeVar;
                    if (VarInfo.TypeKind == StdType.Struct) return string.Format("%struct.{0}", cVarInfo.TypeName);
                }

                return LLVMConverter.ToInstructionName(VarInfo.TypeKind);
            }
        }
        public int Block => (VarInfo != null) ? VarInfo.Block : -1;
        public int Offset => (VarInfo != null) ? VarInfo.Offset : -1;
        public int Length => (VarInfo != null) ? VarInfo.Length : -1;
        public uint PointerLevel => (VarInfo != null) ? VarInfo.PointerLevel : 0;


        protected UseDefChainVar(IRDeclareVar var)
        {
            VarInfo = var;
        }

        public override bool Equals(object obj)
        {
            return obj is UseDefChainVar var &&
                   VarInfo.TypeKind == var.VarInfo.TypeKind &&
                   VarInfo.Name == var.VarInfo.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(VarInfo.TypeKind, VarInfo.Name);
        }

        public abstract void Link(SSAVar toLinkObject);

        public virtual string DebuggerDisplay => IRFormatter.ToDebugFormat(VarInfo);
    }
}
