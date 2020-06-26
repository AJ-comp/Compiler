using Parse.FrontEnd.InterLanguages.Datas;

namespace Parse.FrontEnd.InterLanguages.LLVM.Models
{
    public class GlobalVarItem : TerminalItem, ISSVar
    {
        private IRVar _irVar;

        public string Name => "@" + _irVar.Name;

        public GlobalVarItem(IRVar irvar) : base(irvar.Type)
        {
            _irVar = irvar;
        }
    }
}
