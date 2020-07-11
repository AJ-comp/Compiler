using Parse.FrontEnd.Grammars.Properties;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Datas.Types;

namespace Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.VarDataFormat
{
    public abstract class VarData : IRVar
    {
        public MeaningErrInfoList MeaningErrorList
        {
            get
            {
                MeaningErrInfoList result = new MeaningErrInfoList();

                if (Value.IsUnknown)
                {
                    var convertedLhs = Value;
                    if (convertedLhs.IsOnlyNotInit)
                        result.Add(new MeaningErrInfo(nameof(AlarmCodes.MCL0005), string.Format(AlarmCodes.MCL0005, Name)));
                    else if (convertedLhs.IsNotInitAndDynamicAlloc)
                        result.Add(new MeaningErrInfo(nameof(AlarmCodes.MCL0005), string.Format(AlarmCodes.MCL0005, Name), ErrorType.Warning));
                }

                return result;
            }
        }

        public abstract DType TypeName { get; }
        public abstract string Name { get; }
        public abstract int Block { get; }
        public abstract int Offset { get; }
        public abstract int Length { get; }
        public abstract ValueData Value { get; set; }

        public bool IsMatchWithVarName(string name) => (Name == name);
    }
}
