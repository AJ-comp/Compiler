using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Data
{
    public class UsedVar : IData
    {
        public int Id { get; set; }
        public TokenData NameToken { get; set; }
    }
}
