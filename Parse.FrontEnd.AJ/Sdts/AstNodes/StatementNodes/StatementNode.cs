using AJ.Common;
using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes
{
    public abstract class StatementNode : AJNode, IHasVarList, ISymbolCenter, IExportable<IRExpression>
    {
        public IEnumerable<ISymbolData> SymbolList => _varList;
        public IEnumerable<VariableAJ> VarList => _varList;


        protected StatementNode(AstSymbol node) : base(node)
        {
        }

        protected HashSet<VariableAJ> _varList = new HashSet<VariableAJ>();

        public abstract IRExpression To();
        public abstract IRExpression To(IRExpression from);
    }
}
