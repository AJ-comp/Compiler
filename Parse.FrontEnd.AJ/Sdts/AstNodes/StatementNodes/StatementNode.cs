using Parse.FrontEnd.Ast;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.FrontEnd.AJ.Sdts.Datas.Variables;
using Parse.MiddleEnd.IR.Interfaces;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes
{
    public abstract class StatementNode : AJNode, IHasSymbol, IStmtExpression
    {
        public IEnumerable<ISymbolData> SymbolList => _varList;
        public IEnumerable<VariableMiniC> VarList => _varList;
        public IEnumerable<StatementNode> StatementNodes => _statementNodes;


        protected StatementNode(AstSymbol node) : base(node)
        {
        }

        protected HashSet<VariableMiniC> _varList = new HashSet<VariableMiniC>();
        protected List<StatementNode> _statementNodes = new List<StatementNode>();
    }
}
