using Parse.FrontEnd.Ast;
using System.Security.Cryptography.X509Certificates;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.LiteralNodes
{
    public class CharLiteralNode : LiteralNode
    {
        public char Value { get; }

        public CharLiteralNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Build(SdtsParams param)
        {
            throw new System.NotImplementedException();
        }
    }
}
