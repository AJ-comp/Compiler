using Parse.MiddleEnd.IR.Datas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Data
{
    public class AJVoidType : AJType
    {
        public override uint Size => throw new NotImplementedException();

        public override string GetDebuggerDisplay()
        {
            throw new NotImplementedException();
        }

        public override bool IsArithmeticType() => false;
        public override bool IsFloatingType() => false;
        public override bool IsIncludeType(AJType type) => false;
        public override bool IsIntegerType() => false;

        public override IRType ToIR()
        {
            throw new NotImplementedException();
        }

        public AJVoidType(AJDataType dataType) : base(dataType)
        {
        }
    }
}
