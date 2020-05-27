using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.FrontEnd.Grammars.MiniC
{
    public class MiniCAstBuildParams : AstBuildParams
    {
        public int BlockLevel { get; private set; }
        public int Offset { get; private set; }

        public MiniCAstBuildParams(AstNonTerminal curNode, SymbolTable baseSymbolTable) : base(curNode, baseSymbolTable)
        {
        }
    }
}
