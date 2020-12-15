using Parse.FrontEnd.MiniC.Sdts.Datas.Variables;
using Parse.MiddleEnd.IR.Datas;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Parse.FrontEnd.MiniC.Sdts.Datas
{
    public enum Access { Private, Public };

    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public class ClassData : ISymbolData, IConvertableToIRCode
    {
        public List<SdtsNode> ReferenceTable { get; } = new List<SdtsNode>();
        public string Name => NameToken.Input;

        public Access AccessState { get; }
        public TokenData NameToken { get; }
        public IEnumerable<VariableMiniC> Fields { get; }
        public IEnumerable<FuncData> Funcs { get; }


        public ClassData(Access accessState, TokenData nameToken, IEnumerable<VariableMiniC> fields, IEnumerable<FuncData> funcs)
        {
            AccessState = accessState;
            NameToken = nameToken;
            Fields = fields;
            Funcs = funcs;
        }

        public IRFuncData ToIRFuncData()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            return obj is ClassData data &&
                   Name == data.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }

        public static bool operator ==(ClassData left, ClassData right)
        {
            return EqualityComparer<ClassData>.Default.Equals(left, right);
        }

        public static bool operator !=(ClassData left, ClassData right)
        {
            return !(left == right);
        }

        private string DebuggerDisplay
            => string.Format("class name: {0}, field count: {1}, func count: {2}",
                                        Name,
                                        Fields.Count(),
                                        Funcs.Count());
    }
}
