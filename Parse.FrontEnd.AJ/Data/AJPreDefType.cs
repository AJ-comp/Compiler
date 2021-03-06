using AJ.Common.Helpers;
using Parse.Extensions;
using Parse.MiddleEnd.IR.Datas;
using Parse.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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
                if (DataType == AJDataType.Short) return 2;
                if (DataType == AJDataType.Int) return 4;
                if (DataType == AJDataType.Double) return 8;
                if (DataType == AJDataType.System) return 4;

                return 0;
            }
        }



        public AJPreDefType(AJDataType dataType) : base(dataType)
        {
            DataType = dataType;
        }

        public AJPreDefType(AJDataType dataType, TokenData token) : this(dataType)
        {
            _nameTokens.Add(token);
        }

        public AJPreDefType(AJDataType dataType, TokenDataList tokens) : this(dataType)
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
            if (DataType == AJDataType.Short) return true;
            if (DataType == AJDataType.Int) return true;

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
            if (DataType == AJDataType.Int && preDefType.IsIntegerType()) return true;
            if (DataType == AJDataType.Short && preDefType.IsIntegerType())
            {
                return (preDefType.DataType == AJDataType.Int) ? false : true;
            }
            if (DataType == AJDataType.Byte && preDefType.IsIntegerType())
            {
                return (preDefType.DataType == AJDataType.Byte) ? true : false;
            }

            return false;
        }


        public override IRType ToIR()
        {
            IRType result = null;

            if (DataType == AJDataType.Void) result = new IRType(StdType.Void, PointerDepth);
            else if (DataType == AJDataType.Bool) result = new IRType(StdType.Bit, PointerDepth);
            else if (DataType == AJDataType.Byte) result = new IRType(StdType.Char, PointerDepth);
            else if (DataType == AJDataType.Short) result = new IRType(StdType.Short, PointerDepth);
            else if (DataType == AJDataType.Int) result = new IRType(StdType.Int, PointerDepth);
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
