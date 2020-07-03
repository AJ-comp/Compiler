﻿using Parse.FrontEnd.InterLanguages.Datas.Types;
using Parse.MiddleEnd.IR.Datas;

namespace Parse.MiddleEnd.IR.LLVM.Models
{
    public abstract class LocalVar : ISSVar
    {
        public LocalVar(int offset)
        {
            Offset = offset;
        }

        public string Name => "%" + Offset;
        public int Offset { get; }

        public abstract DataType Type { get; }
        public abstract object Value { get; }
        public abstract bool Signed { get; }
        public abstract bool IsNan { get; }

        public IRVar ToIRVar
        {

        }

        public override bool Equals(object obj)
        {
            return obj is LocalVar var &&
                   Offset == var.Offset;
        }

        public override int GetHashCode()
        {
            return -149965190 + Offset.GetHashCode();
        }
    }

    public class LocalVar<T> : LocalVar where T : DataType
    {
        public LocalVar(int offset) : base(offset)
        {
            
        }

        public override DataType Type => _type;
        public override object Value { get; }
        public override bool Signed { get; }
        public override bool IsNan { get; }

        private T _type;
    }
}
