using Parse.FrontEnd.InterLanguages.Datas;

namespace Parse.FrontEnd.InterLanguages
{
    public class IRFormat
    {
        public IRUnit Unit { get; }
        public IRData IRData { get; }

        public IRFormat(IRUnit unit)
        {
            Unit = unit;
        }

        public IRFormat(IRUnit unit, IRData irData) : this(unit)
        {
            IRData = irData;
        }
    }
}
