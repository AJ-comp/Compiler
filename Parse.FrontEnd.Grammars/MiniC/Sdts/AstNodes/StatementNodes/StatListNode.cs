using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.StatementNodes
{
    public class StatListNode : StatementNode
    {
        public StatListNode(AstSymbol node) : base(node)
        {
        }


        // format summary
        // IfSt | IfElseSt | WhileSt | ExpSt | ReturnSt
        public override SdtsNode Build(SdtsParams param)
        {
            var node = Items[0];
            node.Build(param.CloneForNewBlock());

            if (node is IfStatementNode)
            {

            }
            else if(node is IfElseStatementNode)
            {

            }
            else if(node is WhileStatementNode)
            {

            }
            else if(node is ExprStatementNode)
            {

            }
            else if(node is ReturnStatementNode)
            {

            }

            return this;
        }
    }
}
