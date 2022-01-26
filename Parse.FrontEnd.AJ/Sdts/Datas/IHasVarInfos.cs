using Parse.FrontEnd.AJ.Data;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Sdts.Datas
{
    public interface IHasVarInfos
    {
        IEnumerable<VariableAJ> VarList { get; }
    }
}
