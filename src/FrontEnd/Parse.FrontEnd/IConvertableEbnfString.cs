using System;
using System.Collections.Generic;
using System.Text;

namespace Janglim.FrontEnd
{
    public interface IConvertableEbnfString
    {
        string ToEbnfString(bool bContainLHS = false);
    }
}
