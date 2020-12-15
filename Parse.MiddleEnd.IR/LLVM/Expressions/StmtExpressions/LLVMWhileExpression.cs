using Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions.LogicalExpressions;
using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.StmtExpressions
{
    public class LLVMWhileExpression : LLVMStmtExpression
    {
        public LLVMWhileExpression(LLVMLogicalOpExpression condExpression,
                                            LLVMStmtExpression trueStmtExpression,
                                            LLVMSSATable ssaTable) : base(ssaTable)
        {
            _condExpression = condExpression;
            _trueStmtExpression = trueStmtExpression;
        }

        public override IEnumerable<Instruction> Build()
        {
            CreateInstructionsAndLabels();
            return ArrangeInstructions();
        }

        private void CreateInstructionsAndLabels()
        {
            _startLabel = _ssaTable.RegisterLabel();
            _condInstructions = _condExpression.Build();
            _trueLabel = _ssaTable.RegisterLabel();            // create a true label.

            _trueInstructions = _trueStmtExpression.Build();
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
            result.Add(Instruction.CBranch(_condExpression.Result as BitVariableLLVM,
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


        private LLVMLogicalOpExpression _condExpression;
        private LLVMStmtExpression _trueStmtExpression;

        private BitVariableLLVM _startLabel;
        private BitVariableLLVM _trueLabel;
        private BitVariableLLVM _falseLabel;

        private IEnumerable<Instruction> _condInstructions;
        private IEnumerable<Instruction> _trueInstructions;
    }
}
