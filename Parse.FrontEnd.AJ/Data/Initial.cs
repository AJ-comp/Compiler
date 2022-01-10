using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Data
{
    public class Initial : IData
    {
        public int Id { get; set; }
        public ISymbolData Value { get; set; }

        public ConstantAJ TerminalValue
        {
            get
            {
                if (Value is VariableAJ)
                {
                    return (Value as VariableAJ).InitValue.TerminalValue;
                }
                else if (Value is ConstantAJ) return Value as ConstantAJ;

                return null;
            }
        }
    }
}
