using Parse.Extensions;
using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using Parse.FrontEnd.AJ.Sdts.Datas;
using System.Collections.Generic;

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

        public List<VariableAJ> ParamVarList { get; set; } = new List<VariableAJ>();
        public List<AJNode> Reference { get; set; } = new List<AJNode>();


        public bool IsConstReturn => (ReturnTypeData == null) ? false : ReturnTypeData.Const;
        public AJDataType ReturnType => (ReturnTypeData == null) ? AJDataType.Unknown : ReturnTypeData.DataType;
        public string Name => NameToken.Input;

        public ConstantAJ ReturnValue => ConstantAJ.CreateValueUnknown(ReturnType);


        public IEnumerable<TokenData> FullNameTokens
        {
            get
            {
                List<TokenData> result = new List<TokenData>();

                if (Parent is NamespaceNode)
                {
                    var parent = Parent as NamespaceNode;
                    result.AddRange(parent.NameTokens);
                }
                else if (Parent is ClassDefNode)
                {
                    var parent = Parent as ClassDefNode;
                    result.AddRange(parent.FullNameTokens);
                }

                result.Add(NameToken);

                return result;
            }
        }


        public string FullName => FullNameTokens.ItemsString(PrintType.Property, "Input", ".");


        public bool IsEqualFunction(FuncDefNode target)
        {
            if (Name != target.Name) return false;
            if (ParamVarList.Count != target.ParamVarList.Count) return false;

            for(int i=0; i<ParamVarList.Count; i++)
            {
                var originalParam = ParamVarList[i];
                var targetParam = target.ParamVarList[i];

                if (originalParam.Type != targetParam.Type) return false;
            }

            return true;
        }


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
