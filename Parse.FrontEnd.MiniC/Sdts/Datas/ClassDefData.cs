using Parse.FrontEnd.MiniC.Sdts.Datas.Variables;
using Parse.MiddleEnd.IR.Datas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Parse.FrontEnd.MiniC.Sdts.Datas
{
    public enum Access
    {
        [Description("private")] Private, 
        [Description("public")] Public
    };

    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public class ClassDefData : ISymbolData, IHasSymbol, IClassExpression
    {
        public List<SdtsNode> ReferenceTable { get; } = new List<SdtsNode>();
        public string Name => NameToken.Input;


        // for interface
        public Access AccessType { get; }
        public TokenData NameToken { get; }
        public IEnumerable<IDeclareVarExpression> VarList => Fields;
        public IEnumerable<IFunctionExpression> FuncList => Funcs;



        public int Block { get; private set; }
        public int Offset { get; internal set; }


        public IEnumerable<VariableMiniC> Fields
        {
            get
            {
                List<VariableMiniC> result = new List<VariableMiniC>();

                foreach (var item in SymbolList)
                {
                    if (item is VariableMiniC) result.Add(item as VariableMiniC);
                }

                return result;
            }
        }

        public IEnumerable<FuncDefData> Funcs
        {
            get
            {
                List<FuncDefData> result = new List<FuncDefData>();

                foreach (var item in SymbolList)
                {
                    if (item is FuncDefData)   result.Add(item as FuncDefData);
                }

                return result;
            }
        }

        public IEnumerable<ISymbolData> SymbolList => _symbolDatas;


        public ClassDefData(int blockLevel, Access accessState, TokenData nameToken, HashSet<ISymbolData> symbols)
        {
            Block = blockLevel;

            AccessType = accessState;
            NameToken = nameToken;

            _symbolDatas = symbols;
        }

        public ISymbolData GetMember(Access accessType, string name)
        {
            var result = GetMember(SymbolList, accessType, name);
            if (result != null) return result;

            result = GetMember(SymbolList, accessType, name);

            return result;
        }

        public override bool Equals(object obj)
        {
            return obj is ClassDefData data &&
                   Name == data.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }

        public static bool operator ==(ClassDefData left, ClassDefData right)
        {
            return EqualityComparer<ClassDefData>.Default.Equals(left, right);
        }

        public static bool operator !=(ClassDefData left, ClassDefData right)
        {
            return !(left == right);
        }



        internal void AddSymbol(ISymbolData data) => _symbolDatas.Add(data);
        internal void RemoveSymbol(ISymbolData data) => _symbolDatas.Remove(data);




        private HashSet<ISymbolData> _symbolDatas = new HashSet<ISymbolData>();

        private string DebuggerDisplay
            => string.Format("class name: {0}, field count: {1}, func count: {2}",
                                        Name,
                                        Fields.Count(),
                                        Funcs.Count());


        private ISymbolData GetMember(IEnumerable<ISymbolData> targets, Access access, string name)
        {
            ISymbolData result = null;

            foreach (var target in targets)
            {
                if (target.AccessType != access) continue;
                if (target.Name != name) continue;

                result = target;
                break;
            }

            return result;
        }
    }
}
