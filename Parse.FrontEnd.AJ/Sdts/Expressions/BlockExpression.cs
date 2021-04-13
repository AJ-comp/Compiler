using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Interfaces;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Sdts.Expressions
{
    public class BlockExpression : AJExpression, IRCompoundStatement
    {
        public IEnumerable<IRDeclareVar> LocalVars => _vars;
        public IEnumerable<IRStatement> Statements => _expressions;


        public BlockExpression(ICompoundStmtExpression statement)
        {
            foreach (var localVar in statement.LocalVars)
                _vars.Add(VariableExpression.Create(localVar));
            foreach (var subStatement in statement.Statements)
                _expressions.Add(StatementExpression.Create(subStatement));
        }


        public override string ToString()
        {
            var result = "{" + Environment.NewLine;

            foreach (var localVar in LocalVars)
                result += "\t" + localVar.ToString() + Environment.NewLine;

            foreach (var statement in Statements)
                result += "\t" + statement.ToString() + Environment.NewLine;

            result += "}" + Environment.NewLine;

            return result;
        }


        private List<IRDeclareVar> _vars = new List<IRDeclareVar>();
        private List<IRStatement> _expressions = new List<IRStatement>();
    }
}
