using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts.Datas
{
    public class Array
    {
        public int Id { get; set; }
        public int TypeInfoId { get; set; }
        public int VarId { get; set; }  // ex : [A]
        public int VarType { get; set; }    // Is A declare? or member?
        public int ConstantId { get; set; } // ex : [10]
    }
}
