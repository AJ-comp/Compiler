using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Data
{
    public interface IDeclareable
    {
        IEnumerable<VariableAJ> VarList { get; }
    }
}
