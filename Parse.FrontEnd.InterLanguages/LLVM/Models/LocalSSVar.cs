using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.FrontEnd.InterLanguages.LLVM.Models
{
    public class LocalSSVar : SSVarData
    {
        public override int Offset { get; }
        public override DataType Type { get; }
        public override object LinkedObject { get; internal set; }

        public LocalSSVar(int offset, DataType type)
        {
            Offset = offset;
            Type = type;
        }

        public LocalSSVar(int offset, DataType type, object linkedObject) : this(offset, type)
        {
            LinkedObject = linkedObject;
        }
    }
}
