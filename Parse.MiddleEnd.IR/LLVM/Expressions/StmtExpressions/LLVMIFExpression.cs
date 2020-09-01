using Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions.LogicalExpressions;
using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.StmtExpressions
{
    public class LLVMIFExpression : LLVMStmtExpression
    {
        public LLVMIFExpression(LLVMLogicalOpExpression condExpression, 
                                            LLVMStmtExpression trueStmtExpression, 
                                            LLVMStmtExpression falseStmtExpression,
                                            LLVMSSATable ssaTable) : base(ssaTable)
        {
            _condExpression = condExpression;
            _trueStmtExpression = trueStmtExpression;
            _falseStmtExpression = falseStmtExpression;
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

            _trueInstructions = _trueStmtExpression.Build();
            _falseLabel = _ssaTable.RegisterLabel();            // create a false label.

            if (_falseStmtExpression != null)
            {
                _falseInstructions = _falseStmtExpression.Build();
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

        private LLVMLogicalOpExpression _condExpression;
        private LLVMStmtExpression _trueStmtExpression;
        private LLVMStmtExpression _falseStmtExpression;

        private BitVariableLLVM _trueLabel;
        private BitVariableLLVM _falseLabel;
        private BitVariableLLVM _nextLabel;

        private IEnumerable<Instruction> _condInstructions;
        private IEnumerable<Instruction> _trueInstructions;
        private IEnumerable<Instruction> _falseInstructions;
    }
}
