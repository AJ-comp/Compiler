using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.BackEnd
{
    public abstract class TargetAssembly
    {
        public abstract void GenerateCode(TreeNonTerminal asTree);
    }
}
