using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd
{
    public interface IConvertableEbnfString
    {
        string ToEbnfString(bool bContainLHS = false);
    }
}
