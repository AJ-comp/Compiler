using AJ.Common.Helpers;
using Parse.Extensions;
using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.Types;
using Parse.Types.ConstantTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public partial class FuncDefNode : ISymbolData
    {
        public int StructureId { get; set; }

        public FuncType Type { get; set; }
        public Access AccessType { get; set; }
        public bool IsStatic { get; set; }
        public StatementNode Statement { get; set; }
        public AJTypeInfo TypeData { get; set; }
        public TokenData Token { get; set; }
        public TokenData NameToken { get; set; }
        public int Block { get; set; }
        public int Offset { get; set; }

        public List<VariableAJ> ParamVarList { get; set; } = new List<VariableAJ>();
        public List<AJNode> Reference { get; set; } = new List<AJNode>();


        public bool IsConstReturn => (TypeData == null) ? false : TypeData.Const;
        public AJDataType ReturnType => (TypeData == null) ? AJDataType.Unknown : TypeData.DataType;
        public string Name => NameToken.Input;

        public ConstantAJ ReturnValue => ConstantAJ.CreateValueUnknown(ReturnType);


        public string ToDefineString(bool bDisplayReturnType, bool bDisplayParams)
        {
            string result = $"[{Type}] ";
            if (bDisplayReturnType) result += $"{ReturnType.ToDescription()} ";
            result += Name;

            if (bDisplayParams)
                result += $"({ParamVarList.ItemsString(PrintType.Property, "TypeName")})";

            return result;
        }

        public override bool Equals(object obj)
        {
            var data = obj as FuncDefNode;
            if (data == null) return false;

            if (Type != data.Type) return false;
            if (Name != data.Name) return false;
            if (ParamVarList.Count() != data.ParamVarList.Count) return false;

            for (int i = 0; i < ParamVarList.Count(); i++)
            {
                if (ParamVarList.ElementAt(i) != data.ParamVarList[i]) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            int hash = 0;
            foreach (var param in ParamVarList)
                hash ^= param.DataType.GetHashCode();

            return HashCode.Combine(Type, Name, hash);
        }
        public static bool operator ==(FuncDefNode left, FuncDefNode right)
        {
            return EqualityComparer<FuncDefNode>.Default.Equals(left, right);
        }

        public static bool operator !=(FuncDefNode left, FuncDefNode right)
        {
            return !(left == right);
        }


        private string GetDebuggerDisplay => ToDefineString(true, true);
    }
}
