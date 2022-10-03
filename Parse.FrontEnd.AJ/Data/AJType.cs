using Parse.Extensions;
using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using Parse.MiddleEnd.IR.Datas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Parse.FrontEnd.AJ.Data
{
    public enum AJDataType
    {
        [Description("UnKnown")] Unknown,   // template
        [Description("null")] Null,
        [Description("bool")] Bool,
        [Description("byte")] Byte,
        [Description("sbyte")] SByte,
        [Description("system")] System,
        [Description("void")] Void,
        [Description("short")] Short,
        [Description("ushort")] UShort,
        [Description("int")] Int,
        [Description("uint")] UInt,
        [Description("double")] Double,
        [Description("string")] String,
        [Description("struct")] Struct,
        [Description("class")] Class,
        [Description("enum")] Enum,
    }

    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public abstract class AJType
    {
        protected TokenDataList _nameTokens = new TokenDataList();

        protected AJType(AJDataType dataType, TypeDefNode defineNode)
        {
            DataType = dataType;
            DefineNode = defineNode;
        }

        public TypeDefNode DefineNode { get; }
        public bool Static { get; protected set; } = false;
        public bool Const { get; set; } = false;
        public uint PointerDepth { get; protected set; } = 0;
        public abstract uint Size { get; }
        public IEnumerable<TokenData> NameTokens => _nameTokens;
        public List<int> ArrayLength { get; set; } = new List<int>();
        public AJDataType DataType { get; set; } = AJDataType.Unknown;
        public string Name
        {
            get
            {
                if (DefineNode != null) return DefineNode.Name;
                if (NameTokens.Count() > 0) return NameTokens.Last().Input;

                return string.Empty;
            }
        }
        public string FullName
            => (DefineNode != null) ? DefineNode.FullName : NameTokens.ItemsString();


        public string PopularName => DefineNode?.PopularName;


        public abstract bool IsIntegerType();
        public abstract bool IsFloatingType();
        public abstract bool IsArithmeticType();
        public abstract bool IsIncludeType(AJType type);
        public abstract IRType ToIR();


        /// <summary>
        /// Check if pointer level is 0 and array length is 0.
        /// </summary>
        /// <returns></returns>
        public bool IsNormalType()
        {
            if (PointerDepth != 0) return false;
            if (ArrayLength.Count > 0) return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is AJType info &&
                   DataType == info.DataType &&
                   Name == info.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Static, DataType, Name);
        }

        public static bool operator ==(AJType left, AJType right)
        {
            return EqualityComparer<AJType>.Default.Equals(left, right);
        }

        public static bool operator !=(AJType left, AJType right)
        {
            return !(left == right);
        }

        public abstract string GetDebuggerDisplay();
    }
}
