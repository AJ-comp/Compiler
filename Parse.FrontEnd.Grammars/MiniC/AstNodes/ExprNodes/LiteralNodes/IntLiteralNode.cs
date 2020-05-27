using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.FrontEnd.Grammars.MiniC.AstNodes.ExprNodes.LiteralNodes
{
    public class IntLiteralNode : LiteralNode
    {
        public int Value => System.Convert.ToInt32(Token.Input);

        public IntLiteralNode(TokenData token) : base(token)
        {
        }
    }
}
