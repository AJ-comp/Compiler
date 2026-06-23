using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public abstract class DefinitionNode : AJNode
    {
        protected DefinitionNode(AstSymbol node) : base(node)
        {
        }
    }
}
