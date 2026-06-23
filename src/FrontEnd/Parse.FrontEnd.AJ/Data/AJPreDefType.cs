using AJ.Common.Helpers;
using Parse.Extensions;
using Parse.FrontEnd.AJ.Sdts.AstNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using Parse.MiddleEnd.IR.Datas;
using Parse.Types;
using System.Diagnostics;

namespace Parse.FrontEnd.AJ.Data
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class AJPreDefType : AJType
    {
        public bool Signed { get; set; } = true;
        public bool Nan { get; set; }
        public override uint Size
        {
            get
            {
                if (DataType == AJDataType.Bool) return 1;
                if (DataType == AJDataType.Byte) return 1;
                if (DataType == AJDataType.SByte) return 1;
                if (DataType == AJDataType.Short) return 2;
                if (DataType == AJDataType.UShort) return 2;
                if (DataType == AJDataType.Int) return 4;
                if (DataType == AJDataType.UInt) return 4;
                if (DataType == AJDataType.Double) return 8;
                if (DataType == AJDataType.System) return 4;

                return 0;
            }
        }


        public AJPreDefType(AJDataType dataType, TypeDefNode defineNode) : base(dataType, defineNode)
        {
            DataType = dataType;
        }

        public AJPreDefType(AJDataType dataType, TypeDefNode defineNode, TokenData token) : this(dataType, defineNode)
        {
            _nameTokens.Add(token);
        }

        public AJPreDefType(AJDataType dataType, TypeDefNode defineNode, TokenDataList tokens) : this(dataType, defineNode)
        {
            _nameTokens = tokens;
        }


        /***********************************************/
        /// <summary>
        /// This function checks whether the type is the value type.
        /// </summary>
        /// <returns></returns>
        /***********************************************/
        public bool IsValueType()
        {
            if (DataType == AJDataType.Unknown) return false;
            return !IsReferenceType();
        }

        /***********************************************/
        /// <summary>
        /// This function checks whether the type is the reference type.
        /// </summary>
        /// <returns></returns>
        /***********************************************/
        public bool IsReferenceType()
        {
            if (DataType == AJDataType.Class) return true;
            if (DataType == AJDataType.String) return true;
            if (ArrayLength.Count > 0) return true;

            return false;
        }

        /***********************************************/
        /// <summary>
        /// This function checks whether the type is a integer type.
        /// </summary>
        /// <returns></returns>
        /***********************************************/
        public override bool IsIntegerType()
        {
            if (DataType == AJDataType.Byte) return true;
            if (DataType == AJDataType.SByte) return true;
            if (DataType == AJDataType.Short) return true;
            if (DataType == AJDataType.UShort) return true;
            if (DataType == AJDataType.Int) return true;
            if (DataType == AJDataType.UInt) return true;

            return false;
        }

        /***********************************************/
        /// <summary>
        /// This function checks whether the type is a floating type.
        /// </summary>
        /// <returns></returns>
        /***********************************************/
        public override bool IsFloatingType() => DataType == AJDataType.Double;

        /***********************************************/
        /// <summary>
        /// This function checks whether the type is a arithmetic type. <br/>
        /// The arithmetic type is integer type and floating type.
        /// </summary>
        /// <returns></returns>
        /***********************************************/
        public override bool IsArithmeticType()
        {
            if (IsIntegerType()) return true;
            if (IsFloatingType()) return true;

            return false;
        }


        public bool IsSameType(AJType target)
        {
            if (DataType != target.DataType) return false;
            if (ArrayLength.Count != target.ArrayLength.Count) return false;
            if (PointerDepth != target.PointerDepth) return false;

            return true;
        }


        public override bool IsIncludeType(AJType type)
        {
            if (type is AJUserDefType) return false;
            if (DataType == AJDataType.Void) return false;

            var preDefType = type as AJPreDefType;
            if (IsSameType(preDefType)) return true;

            if (!IsNormalType()) return false;

            // if normal type
            if (IsFloatingType() && preDefType.IsArithmeticType()) return true;
            if (DataType == AJDataType.Int || DataType == AJDataType.UInt)
            {
                if (preDefType.IsIntegerType()) return true;
            }
            if (DataType == AJDataType.Short || DataType == AJDataType.UShort)
            {
                if (preDefType.IsIntegerType())
                    return (preDefType.DataType == AJDataType.Int || preDefType.DataType == AJDataType.UInt) ? false : true;
            }
            if (DataType == AJDataType.Byte && DataType == AJDataType.SByte)
            {
                return (preDefType.DataType == AJDataType.Byte || preDefType.DataType == AJDataType.SByte) ? true : false;
            }

            return false;
        }


        public override IRType ToIR()
        {
            IRType result = null;

            if (DataType == AJDataType.Void) result = new IRType(StdType.Void, PointerDepth);
            else if (DataType == AJDataType.Bool) result = new IRType(StdType.Bit, PointerDepth);
            else if (DataType == AJDataType.Byte) result = new IRType(StdType.Char, PointerDepth);
            else if (DataType == AJDataType.SByte) result = new IRType(StdType.UChar, PointerDepth);
            else if (DataType == AJDataType.Short) result = new IRType(StdType.Short, PointerDepth);
            else if (DataType == AJDataType.UShort) result = new IRType(StdType.UShort, PointerDepth);
            else if (DataType == AJDataType.Int) result = new IRType(StdType.Int, PointerDepth);
            else if (DataType == AJDataType.UInt) result = new IRType(StdType.UInt, PointerDepth);
            else if (DataType == AJDataType.Double) result = new IRType(StdType.Double, PointerDepth);
            else if (DataType == AJDataType.String) result = new IRType(StdType.Char, PointerDepth);
            else if (DataType == AJDataType.Unknown) result = new IRType(StdType.Unknown, PointerDepth);

            result.Name = Name;
            result.Signed = Signed;
            result.Size = Size;
            result.Nan = Nan;
            result.ArrayLength.AddRange(ArrayLength);

            return result;
        }

        public override string GetDebuggerDisplay()
        {
            string result = Static ? "static " : string.Empty;
            result += Const ? "const " : string.Empty;

            result += $"{DataType.ToDescription()} (size: {Size})";
            result += PointerDepth.ToAnyStrings("*");
            foreach (var arrayLength in ArrayLength) result += $"[{arrayLength}]";

            if (IsFloatingType()) result += $"(nan: {Nan})";

            return result;
        }
    }
}
