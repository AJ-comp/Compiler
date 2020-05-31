using Parse.FrontEnd.Ast;
using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.MiniC
{
    public class MiniCAstBuildParams : AstBuildParams
    {
        public int BlockLevel { get; internal set; }
        public int Offset { get; internal set; }
        public AstBuildOption BuildOption { get; internal set; } = AstBuildOption.None;

        public MiniCAstBuildParams(SymbolTable baseSymbolTable) : base(baseSymbolTable)
        {
        }

        public MiniCAstBuildParams(SymbolTable baseSymbolTable, int blockLevel, int offset) : this(baseSymbolTable)
        {
            BlockLevel = blockLevel;
            Offset = offset;
        }

        public MiniCAstBuildParams(SymbolTable baseSymbolTable, int blockLevel, int offset, AstBuildOption astBuildOption) : this(baseSymbolTable, blockLevel, offset)
        {
            BuildOption = astBuildOption;
        }

        public override object Clone()
        {
            return new MiniCAstBuildParams(SymbolTable, BlockLevel, Offset, BuildOption);
        }
    }
}
