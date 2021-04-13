using Parse.FrontEnd.AJ.Sdts.Datas.Variables;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Sdts.Datas
{
    public interface IHasVarInfos
    {
        IEnumerable<VariableMiniC> VarList { get; }
    }
}
