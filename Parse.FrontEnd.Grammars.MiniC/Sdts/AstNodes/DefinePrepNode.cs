using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes
{
    public class DefinePrepNode : MiniCNode
    {
        public TokenData Ident { get; private set; }
        public ExprNode Expression { get; private set; }

        public DefinePrepNode(AstSymbol node) : base(node)
        {
        }

        // #define [0]
        // Ident [1]
        // ExprNode [2]
        public override SdtsNode Build(SdtsParams param)
        {
            Ident = (Items[1].Build(param) as TerminalNode).Token;
            Expression = Items[2].Build(param) as ExprNode;

//            SymbolTable.DefineTable.CreateNewBlock(new DefinePrepData(Ident.Input, )

            return this;
        }
    }
}
