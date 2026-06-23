using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.Expressions
{
    public class IRProgramRoot : IRExpression
    {
        public string NamespaceName { get; set; }

        public List<IRStructDef> StructDefs { get; } = new List<IRStructDef>();
        public List<IRFunction> Functions { get; } = new List<IRFunction>();
    }
}
