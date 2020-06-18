using Parse.FrontEnd.InterLanguages.Datas;

namespace Parse.FrontEnd.InterLanguages
{
    public class IRFormat
    {
        public IRUnit Unit { get; }
        public VarData VarData { get; }

        public IRFormat(IRUnit unit)
        {
            Unit = unit;
        }

        public IRFormat(IRUnit unit, VarData varData) : this(unit)
        {
            VarData = varData;
        }
    }
}
