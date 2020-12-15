using Parse.FrontEnd.MiniC.Sdts.Datas.Variables;
using Parse.MiddleEnd.IR.Datas;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Parse.FrontEnd.MiniC.Sdts.Datas
{
    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public class FuncData : ISymbolData, IConvertableToIRCode
    {
        public MiniCDataType ReturnType => TypeData.DataType;
        public string Name => NameToken.Input;

        public Access AccessType { get; internal set; }
        public MiniCTypeInfo TypeData { get; internal set; }
        public TokenData NameToken { get; internal set; }
        public int Offset { get; internal set; }
        public List<VariableMiniC> ParamVars { get; } = new List<VariableMiniC>();
        public List<SdtsNode> ReferenceTable { get; } = new List<SdtsNode>();

        public FuncData(Access accessType, MiniCTypeInfo typeData, TokenData nameToken, int offset, IEnumerable<VariableMiniC> paramVars)
        {
            AccessType = accessType;
            TypeData = typeData;
            NameToken = nameToken;
            Offset = offset;
            ParamVars.AddRange(paramVars);
        }


        public string ToDefineString(bool bDisplayReturnType, bool bDisplayParams)
        {
            string result = (bDisplayReturnType) ? string.Format("{0} {1}", Helper.GetDescription(ReturnType), Name)
                                                                 : Name;

            if (bDisplayParams)
            {
                result += "(";
                foreach (var param in ParamVars)
                    result += param.DataType.ToString() + ",";

                result = result.Substring(0, result.Length - 1) + ")";
            }

            return result;
        }

        public IRFuncData ToIRFuncData()
        {
            List<IRVar> paramVars = new List<IRVar>();

            foreach (var var in ParamVars)
            {
                if (var.VariableProperty == VarProperty.Param) paramVars.Add(var);
            }

            return new IRFuncData(paramVars,
                                                TypeData.Const,
                                                IRConverter.ToIRReturnType(ReturnType),
                                                Name,
                                                0);
        }

        public override bool Equals(object obj)
        {
            var data = obj as FuncData;
            if (data == null) return false;

            if (Name != data.Name) return false;
            if (ParamVars.Count != data.ParamVars.Count) return false;

            for (int i = 0; i < ParamVars.Count; i++)
            {
                if (ParamVars[i] != data.ParamVars[i]) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            List<MiniCDataType> paramTypes = new List<MiniCDataType>();

            int hash = 0;
            foreach (var param in ParamVars)
                hash ^= param.DataType.GetHashCode();

            return HashCode.Combine(Name, hash);
        }

        private string DebuggerDisplay => ToDefineString(true, true);

        public static bool operator ==(FuncData left, FuncData right)
        {
            return EqualityComparer<FuncData>.Default.Equals(left, right);
        }

        public static bool operator !=(FuncData left, FuncData right)
        {
            return !(left == right);
        }
    }
}
