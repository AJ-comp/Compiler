using Parse.Extensions;
using Parse.FrontEnd.MiniC.Sdts.Datas.Variables;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Interfaces;
using Parse.Types;
using Parse.Types.ConstantTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Parse.FrontEnd.MiniC.Sdts.Datas
{
    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public class FuncDefData : ISymbolData, IFunctionExpression
    {
        /****************** for expression interface ***********************/
        public IClassExpression PartyInfo { get; }
        public Access AccessType { get; internal set; }
        public bool IsStatic { get; }
        public bool IsConstReturn => TypeData.Const;
        public MiniCDataType ReturnType => TypeData.DataType;
        public string Name => NameToken.Input;
        public IStmtExpression Statement { get; internal set; }
        public IEnumerable<IDeclareVarExpression> ParamVars => ParamVarList;
        /***********************************************************/


        public IConstant ReturnValue
        {
            get
            {
                if (ReturnType == MiniCDataType.Int) return new IntConstant(0, State.Unknown);
                //                else if(ReturnType == MiniCDataType.Address) return new PointerVariableMiniC()

                return new UnknownConstant();
            }
        }

        public MiniCTypeInfo TypeData { get; internal set; }
        public TokenData NameToken { get; internal set; }
        public int Block { get; private set; }
        public int Offset { get; internal set; }
        public string PartyName { get; set; }
        public List<VariableMiniC> ParamVarList { get; } = new List<VariableMiniC>();
        public List<SdtsNode> ReferenceTable { get; } = new List<SdtsNode>();

        public FuncDefData(int blockLevel,
                                int offset,
                                bool isStatic,
                                IClassExpression partyInfo,
                                Access accessType,
                                MiniCTypeInfo typeData,
                                TokenData nameToken,
                                IEnumerable<VariableMiniC> paramVars,
                                IStmtExpression statement)
        {
            Block = blockLevel;
            Offset = offset;
            IsStatic = isStatic;

            PartyInfo = partyInfo;
            AccessType = accessType;
            TypeData = typeData;
            NameToken = nameToken;
            ParamVarList.AddRange(paramVars);
            Statement = statement;
        }


        public string ToDefineString(bool bDisplayReturnType, bool bDisplayParams)
        {
            string result = (bDisplayReturnType) ? string.Format("{0} {1}", Helper.GetDescription(ReturnType), Name)
                                                                 : Name;

            if (bDisplayParams)
            {
                result += "(";
                result += ParamVarList.ItemsString(PrintType.Property, "TypeKind");
                //                foreach (var param in ParamVars)
                //                    result += param.TypeKind.ToString() + ",";

                result = result.Substring(0, result.Length - 1) + ")";
            }

            return result;
        }

        public override bool Equals(object obj)
        {
            var data = obj as FuncDefData;
            if (data == null) return false;

            if (Name != data.Name) return false;
            if (ParamVarList.Count != data.ParamVarList.Count) return false;

            for (int i = 0; i < ParamVarList.Count; i++)
            {
                if (ParamVarList[i] != data.ParamVarList[i]) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            int hash = 0;
            foreach (var param in ParamVarList)
                hash ^= param.TypeKind.GetHashCode();

            return HashCode.Combine(Name, hash);
        }

        private string DebuggerDisplay => ToDefineString(true, true);

        public static bool operator ==(FuncDefData left, FuncDefData right)
        {
            return EqualityComparer<FuncDefData>.Default.Equals(left, right);
        }

        public static bool operator !=(FuncDefData left, FuncDefData right)
        {
            return !(left == right);
        }
    }
}
