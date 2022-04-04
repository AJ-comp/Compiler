using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Data
{
    public class FunctionAJ
    {
        public int Id { get; set; }
        public Access AccessType { get; internal set; }
        public AJType Type { get; set; }
    }
}
