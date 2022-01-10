using Parse.FrontEnd.AJ.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts
{
    public interface IHasVarList
    {
        IEnumerable<VariableAJ> VarList { get; }
    }
}
