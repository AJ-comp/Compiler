using Parse.FrontEnd.MiniC.Sdts.Datas.Variables;
using System.Collections.Generic;

namespace Parse.FrontEnd.MiniC.Sdts.Datas
{
    public interface IHasVarInfos
    {
        IEnumerable<VariableMiniC> VarList { get; }
    }
}
