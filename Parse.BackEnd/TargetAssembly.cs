using Parse.FrontEnd.Ast;

namespace Parse.BackEnd
{
    public abstract class TargetAssembly
    {
        public abstract void GenerateCode(ParseTreeNonTerminal asTree);
    }
}
