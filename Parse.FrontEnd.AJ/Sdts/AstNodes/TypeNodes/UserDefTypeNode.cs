using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes
{
    public abstract class UserDefTypeNode : DataTypeNode
    {
        protected UserDefTypeNode(AstSymbol node) : base(node)
        {
        }
    }
}
