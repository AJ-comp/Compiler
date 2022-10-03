using AJ.Common.Helpers;
using Parse.Extensions;
using Parse.MiddleEnd.IR.Datas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Data
{
    public class AJUnknownType : AJType
    {
        public AJUnknownType() : base(AJDataType.Unknown, null)
        {
        }

        public override uint Size => 0;

        public override string GetDebuggerDisplay()
        {
            string result = Static ? "static " : string.Empty;

            result += $"{DataType.ToDescription()} ({FullName}) (size: ?)";
            result += PointerDepth.ToAnyStrings("*");
            foreach (var arrayLength in ArrayLength) result += $"[{arrayLength}]";

            return result;
        }

        public override bool IsArithmeticType()
        {
            return false;
        }

        public override bool IsFloatingType()
        {
            return false;
        }

        public override bool IsIncludeType(AJType type)
        {
            return false;
        }

        public override bool IsIntegerType()
        {
            return false;
        }

        public override IRType ToIR()
        {
            throw new NotImplementedException();
        }
    }
}
