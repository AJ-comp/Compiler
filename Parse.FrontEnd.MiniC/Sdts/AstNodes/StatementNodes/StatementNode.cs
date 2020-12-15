using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Sdts.Datas;
using Parse.FrontEnd.MiniC.Sdts.Datas.Variables;
using System.Collections.Generic;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.StatementNodes
{
    public abstract class StatementNode : MiniCNode, IHasVarInfos
    {
        public IEnumerable<VariableMiniC> VarList => _varList;

        protected StatementNode(AstSymbol node) : base(node)
        {
        }

        protected HashSet<VariableMiniC> _varList = new HashSet<VariableMiniC>();
    }
}
