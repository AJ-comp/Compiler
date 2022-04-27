using AJ.Common;
using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes
{
    public abstract class StatementNode : AJNode, IHasVarList, ISymbolCenter, IExportable<IRExpression>
    {
        public IEnumerable<ISymbolData> SymbolList => _varList;
        public IEnumerable<VariableAJ> VarList => _varList;

        public bool IsEmpty => Items.Count == 0;

        /// <summary>
        /// This means whether clarify the return statement in a statement.
        /// </summary>
        public bool ClarifyReturn { get; protected set; } = false;


        protected StatementNode(AstSymbol node) : base(node)
        {
        }

        protected HashSet<VariableAJ> _varList = new HashSet<VariableAJ>();

        public abstract IRExpression To();
        public abstract IRExpression To(IRExpression from);

        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            base.CompileLogic(param);

            if (IsEmpty) AddEmptyStatementWarning();

            return this;
        }


        protected void AddEmptyStatementWarning()
        {
            Alarms.Add(new MeaningErrInfo(AllTokens, nameof(AlarmCodes.AJ0033), AlarmCodes.AJ0033, ErrorType.Warning));
        }
    }
}
