using AJ.Common.Helpers;
using Parse.Extensions;
using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using Parse.MiddleEnd.IR.Datas;
using Parse.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Parse.FrontEnd.AJ.Data
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class AJUserDefType : AJType
    {
        public override uint Size => (PointerDepth > 0) ? 4 : DefineNode.Size;

        public AJUserDefType(TypeDefNode defType, TokenDataList tokens = null) : base(AJDataType.Unknown, defType)
        {
            _nameTokens.AddRangeExceptNull(tokens);

            if (DefineNode == null) DataType = AJDataType.Unknown;
            else if (DefineNode is ClassDefNode) DataType = AJDataType.Class;
        }


        public static AJType CreateThisType(TypeDefNode typeNode)
        {
            var result = new AJUserDefType(typeNode);

            result.Static = false;
            result.PointerDepth = 1;

            return result;
        }


        public override bool IsIncludeType(AJType type)
        {
            return this == type;
        }

        public override bool IsIntegerType()
        {
            return false;
        }

        public override bool IsFloatingType()
        {
            return false;
        }

        public override bool IsArithmeticType()
        {
            return false;
        }


        public override IRType ToIR()
        {
            IRType result = null;

            if (DataType == AJDataType.Enum) result = new IRType(StdType.Enum, PointerDepth);
            else if (DataType == AJDataType.Struct) result = new IRType(StdType.Struct, PointerDepth);
            else if (DataType == AJDataType.Class) result = new IRType(StdType.Struct, PointerDepth);
            else if (DataType == AJDataType.Unknown) result = new IRType(StdType.Unknown, PointerDepth);

            result.Name = FullName;
            result.Size = Size;
            result.ArrayLength.AddRange(ArrayLength);

            return result;
        }


        public override string GetDebuggerDisplay()
        {
            string result = Static ? "static " : string.Empty;

            result += $"{DataType.ToDescription()} ({FullName}) (size: {Size})";
            result += PointerDepth.ToAnyStrings("*");
            foreach (var arrayLength in ArrayLength) result += $"[{arrayLength}]";

            return result;
        }
    }
}
