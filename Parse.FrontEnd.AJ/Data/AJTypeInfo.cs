using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using Parse.MiddleEnd.IR.Datas;
using Parse.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Parse.FrontEnd.AJ.Data
{
    public enum AJDataType
    {
        [Description("UnKnown")] Unknown,   // template
        [Description("bool")] Bool,
        [Description("byte")] Byte,
        [Description("system")] System,
        [Description("void")] Void,
        [Description("short")] Short,
        [Description("int")] Int,
        [Description("double")] Double,
        [Description("string")] String,
        [Description("struct")] Struct,
        [Description("class")] Class,
        [Description("enum")] Enum,
    }

    public partial class AJTypeInfo : IData
    {
        public int Id { get; set; } = 0;
        public DataTypeNode DefineNode { get; set; }
        public bool Static { get; set; } = false;
        public bool Const { get; set; } = false;
        public TokenData Token { get; set; }
        public uint PointerDepth { get; set; } = 0;
        public uint Size { get; set; }
        public bool Signed { get; set; } = true;
        public List<int> ArrayLength { get; set; } = new List<int>();
        public bool Nan { get; set; }
        public AJDataType DataType { get; set; } = AJDataType.Unknown;

        public string Name => Token.Input;


        public AJTypeInfo(AJDataType dataType)
        {
            DataType = dataType;
            Size = GetSize();
        }

        public AJTypeInfo(AJDataType dataType, TokenData token) : this(dataType)
        {
            Token = token;
        }


        public static AJTypeInfo CreateThisType(AJDataType type)
        {
            var result = new AJTypeInfo(type);

            result.Static = false;
            result.Const = true;
            result.Token = null;
            result.PointerDepth = 1;
            result.Size = 4;
            result.DataType = type;

            return result;
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
        public bool IsIntegerType()
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
        public bool IsFloatingType() => DataType == AJDataType.Double;

        /***********************************************/
        /// <summary>
        /// This function checks whether the type is a arithmetic type. <br/>
        /// The arithmetic type is integer type and floating type.
        /// </summary>
        /// <returns></returns>
        /***********************************************/
        public bool IsArithmeticType()
        {
            if (IsIntegerType()) return true;
            if (IsFloatingType()) return true;

            return false;
        }


        public IRType ToIR()
        {
            IRType result = null;

            if (DataType == AJDataType.Void) result = new IRType(StdType.Void, PointerDepth);
            else if (DataType == AJDataType.Bool) result = new IRType(StdType.Bit, PointerDepth);
            else if (DataType == AJDataType.Byte) result = new IRType(StdType.Char, PointerDepth);
            else if (DataType == AJDataType.Short) result = new IRType(StdType.Short, PointerDepth);
            else if (DataType == AJDataType.Int) result = new IRType(StdType.Int, PointerDepth);
            else if (DataType == AJDataType.Double) result = new IRType(StdType.Double, PointerDepth);
            else if (DataType == AJDataType.Enum) result = new IRType(StdType.Enum, PointerDepth);
            else if (DataType == AJDataType.Struct) result = new IRType(StdType.Struct, PointerDepth);
            else if (DataType == AJDataType.Class) result = new IRType(StdType.Struct, PointerDepth);
            else if (DataType == AJDataType.String) result = new IRType(StdType.Char, PointerDepth);
            else if (DataType == AJDataType.Unknown) result = new IRType(StdType.Unknown, PointerDepth);

            result.Name = Name;
            result.Signed = Signed;
            result.Size = Size;
            result.Nan = Nan;
            result.ArrayLength.AddRange(ArrayLength);

            return result;
        }


        public override bool Equals(object obj)
        {
            return obj is AJTypeInfo info &&
                   Static == info.Static &&
                   DataType == info.DataType &&
                   Name == info.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Static, DataType, Name);
        }

        public static bool operator ==(AJTypeInfo left, AJTypeInfo right)
        {
            return EqualityComparer<AJTypeInfo>.Default.Equals(left, right);
        }

        public static bool operator !=(AJTypeInfo left, AJTypeInfo right)
        {
            return !(left == right);
        }


        private uint GetSize()
        {
            if (DataType == AJDataType.Bool) return 1;
            if (DataType == AJDataType.Byte) return 1;
            if (DataType == AJDataType.Short) return 2;
            if (DataType == AJDataType.Int) return 4;
            if (DataType == AJDataType.Double) return 8;
            if (DataType == AJDataType.System) return 4;
            if (DataType == AJDataType.String) return 0;
            if (DataType == AJDataType.Class) return 0;
            if (DataType == AJDataType.Struct) return 0;
            if (DataType == AJDataType.Enum) return 0;

            return 0;
        }
    }
}
