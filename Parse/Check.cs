using Parse.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse
{
    public class Check
    {
        public static bool IsUserDefType(StdType type)
        {
            if (type == StdType.Struct) return true;
            else if (type == StdType.Enum) return true;

            return false;
        }
    }
}
