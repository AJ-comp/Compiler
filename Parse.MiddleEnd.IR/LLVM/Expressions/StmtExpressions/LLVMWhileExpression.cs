using Parse.MiddleEnd.IR.Interfaces;
using Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions;
using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.StmtExpressions
{
    public class LLVMWhileExpression : LLVMStmtExpression
    {
        public LLVMWhileExpression(IRWhileStatement statement, LLVMSSATable ssaTable) : base(ssaTable)
        {
            _statement = statement;
            _condExpr = new LLVMCompareOpExpression(statement.CondExpr, ssaTable);
            _trueStmt = Create(statement.TrueStatement, ssaTable);
        }

        public override IEnumerable<Instruction> Build()
        {
            CreateInstructionsAndLabels();
            return ArrangeInstructions();
        }

        private void CreateInstructionsAndLabels()
        {
            _startLabel = _ssaTable.RegisterLabel();
            _condInstructions = _condExpr.Build();
            _trueLabel = _ssaTable.RegisterLabel();            // create a true label.

            _trueInstructions = _trueStmt.Build();
            _falseLabel = _ssaTable.RegisterLabel();            // create a false label.
        }

        private IEnumerable<Instruction> ArrangeInstructions()
        {
            List<Instruction> result = new List<Instruction>
            {
                Instruction.UCBranch(_startLabel),
                Instruction.EmptyLine(),
                Instruction.EmptyLine(_startLabel.Name)
            };

            result.AddRange(_condInstructions);
            result.Add(Instruction.CBranch(_condExpr.Result as BitVariableLLVM,
                                                            _trueLabel,
                                                            _falseLabel));

            result.Add(Instruction.EmptyLine());
            result.Add(Instruction.EmptyLine(_trueLabel.Name));
            result.AddRange(_trueInstructions);
            result.Add(Instruction.UCBranch(_startLabel));

            result.Add(Instruction.EmptyLine());
            result.Add(Instruction.EmptyLine(_falseLabel.Name));

            return result;
        }

        private IRWhileStatement _statement;
        private LLVMCompareOpExpression _condExpr;
        private LLVMStmtExpression _trueStmt;

        private BitVariableLLVM _startLabel;
        private BitVariableLLVM _trueLabel;
        private BitVariableLLVM _falseLabel;

        private IEnumerable<Instruction> _condInstructions;
        private IEnumerable<Instruction> _trueInstructions;
    }
}
