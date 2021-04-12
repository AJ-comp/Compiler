using Parse.MiddleEnd.IR.Interfaces;
using Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions;
using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.StmtExpressions
{
    public class LLVMIFExpression : LLVMStmtExpression
    {
        public LLVMIFExpression(IRIFStatement statement, LLVMSSATable ssaTable) : base(ssaTable)
        {
            _statement = statement;

            _condExpression = new LLVMCompareOpExpression(statement.CondExpr, ssaTable);
            _trueStmt = Create(statement.TrueStatement, ssaTable);
        }

        public LLVMIFExpression(IRIFElseStatement statement, LLVMSSATable ssaTable)
            : this(statement as IRIFStatement, ssaTable)
        {
            _falseStmt = Create(statement.FalseStatement, ssaTable);
        }

        public override IEnumerable<Instruction> Build()
        {
            CreateInstructionsAndLabels();
            return ArrangeInstructions();
        }

        private void CreateInstructionsAndLabels()
        {
            _condInstructions = _condExpression.Build();
            _trueLabel = _ssaTable.RegisterLabel();            // create a true label.

            _trueInstructions = _trueStmt.Build();
            _falseLabel = _ssaTable.RegisterLabel();            // create a false label.

            if (_falseStmt != null)
            {
                _falseInstructions = _falseStmt.Build();
                _nextLabel = _ssaTable.RegisterLabel();            // create a next label.
            }
            else _nextLabel = _falseLabel;
        }

        private IEnumerable<Instruction> ArrangeInstructions()
        {
            List<Instruction> result = new List<Instruction>();

            result.AddRange(_condInstructions);
            result.Add(Instruction.CBranch(_condExpression.Result as BitVariableLLVM,
                                                            _trueLabel,
                                                            _falseLabel));

            result.Add(Instruction.EmptyLine());
            result.Add(Instruction.EmptyLine(_trueLabel.Name));
            result.AddRange(_trueInstructions);
            result.Add(Instruction.UCBranch(_nextLabel));

            result.Add(Instruction.EmptyLine());
            result.Add(Instruction.EmptyLine(_falseLabel.Name));
            if (_falseInstructions != null)
            {
                result.AddRange(_falseInstructions);
                result.Add(Instruction.UCBranch(_nextLabel));
                result.Add(Instruction.EmptyLine());
                result.Add(Instruction.EmptyLine(_nextLabel.Name));
            }

            return result;
        }

        private IRIFStatement _statement;

        private LLVMCompareOpExpression _condExpression;
        private LLVMStmtExpression _trueStmt;
        private LLVMStmtExpression _falseStmt;

        private BitVariableLLVM _trueLabel;
        private BitVariableLLVM _falseLabel;
        private BitVariableLLVM _nextLabel;

        private IEnumerable<Instruction> _condInstructions;
        private IEnumerable<Instruction> _trueInstructions;
        private IEnumerable<Instruction> _falseInstructions;
    }
}
