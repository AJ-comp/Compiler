using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Data
{
    public class PreDefTypeData
    {
        public string ShortName;
        public string DefineFullName;

        public uint Size;

        public PreDefTypeData(string shortName, string defineFullName, uint size)
        {
            ShortName = shortName;
            DefineFullName = defineFullName;
            Size = size;
        }
    }
}
