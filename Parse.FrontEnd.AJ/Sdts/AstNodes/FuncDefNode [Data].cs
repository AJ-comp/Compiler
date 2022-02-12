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
    public partial class FuncDefNode : ISymbolData
    {
        public int StructureId { get; set; }

        public FuncType Type { get; set; } = FuncType.Normal;
        public Access AccessType { get; set; } = Access.Private;
        public bool IsStatic { get; set; }
        public StatementNode Statement { get; set; }
        public AJTypeInfo ReturnTypeData { get; set; }
        public TokenData NameToken { get; set; }
        public int Block { get; set; }
        public int Offset { get; set; }

        public List<VariableAJ> ParamVarList { get; set; } = new List<VariableAJ>();
        public List<AJNode> Reference { get; set; } = new List<AJNode>();


        public bool IsConstReturn => (ReturnTypeData == null) ? false : ReturnTypeData.Const;
        public AJDataType ReturnType => (ReturnTypeData == null) ? AJDataType.Unknown : ReturnTypeData.DataType;
        public string Name => NameToken.Input;

        public ConstantAJ ReturnValue => ConstantAJ.CreateValueUnknown(ReturnType);


        public string ToDefineString(bool bDisplayReturnType, bool bDisplayParams)
        {
            string result = string.Empty;
            if (bDisplayReturnType) result += $"{ReturnTypeData.Name} ";
            result += Name;

            if (bDisplayParams)
                result += $"({ParamVarList.ItemsString(PrintType.Property, "Type")})";

            return result;
        }


        public override string ToString()
        {
            string result = $"[{Type}] {AccessType} {ReturnType} {Name} ";
            result += "(";
            foreach (var paramVar in ParamVarList)
            {
                result += $"{paramVar.Type.GetDebuggerDisplay()} ";
            }
            result += ")";

            return result;
        }
    }
}
