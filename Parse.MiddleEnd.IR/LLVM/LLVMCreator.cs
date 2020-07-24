using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Datas.Types;
using Parse.MiddleEnd.IR.LLVM.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.LLVM
{
    public class LLVMCreator
    {
        public static LocalVar CreateLocalVar(DType typeName, int offset)
        {
            if (typeName == DType.Bit) return new LocalVar<Bit>(offset);
            if (typeName == DType.Byte) return new LocalVar<Datas.Types.Byte>(offset);
            if (typeName == DType.Short) return new LocalVar<Short>(offset);
            if (typeName == DType.Int) return new LocalVar<Int>(offset);
            if (typeName == DType.Double) return new LocalVar<DoubleType>(offset);

            return null;
        }

        public static GlobalVar CreateGlobalVar(DType typeName, IRVar irVar)
        {
            if (typeName == DType.Bit) return new GlobalVar<Bit>(irVar);
            if (typeName == DType.Byte) return new GlobalVar<Datas.Types.Byte>(irVar);
            if (typeName == DType.Short) return new GlobalVar<Short>(irVar);
            if (typeName == DType.Int) return new GlobalVar<Int>(irVar);
            if (typeName == DType.Double) return new GlobalVar<DoubleType>(irVar);

            return null;
        }
    }
}
