using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.Expressions
{
    public class IRStructDef : IRExpression
    {
        public string Name { get; set; }
        public List<IRVariable> Members { get; } = new List<IRVariable>();


        public string IRName => Name.Replace(".", "_");
    }
}
