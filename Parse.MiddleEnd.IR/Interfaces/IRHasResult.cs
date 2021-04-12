using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.Interfaces
{
    public interface IRHasResult<T>
    {
        T Result { get; }
    }
}
