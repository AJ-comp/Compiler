using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions
{
    public class LLVMBlockExpression : LLVMDependencyExpression
    {
        public IReadOnlyList<LLVMExpression> Expressions => _expressions;

        public LLVMBlockExpression(IEnumerable<LLVMExpression> expressions, 
                                                    LLVMSSATable ssaTable) : base(ssaTable)
        {
            _expressions.AddRange(expressions);
        }

        public LLVMBlockExpression(LLVMSSATable ssaTable) : base(ssaTable)
        {
        }

        public void AddItem(LLVMExpression expression)
        {
            _expressions.Add(expression);
        }

        public void AddItems(IEnumerable<LLVMExpression> expressions)
        {
            _expressions.AddRange(expressions);
        }

        public void AddBlock(LLVMBlockExpression blockExpression)
        {
            foreach (var block in blockExpression.Expressions)
            {
                _expressions.Add(block);
            }
        }

        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> result = new List<Instruction>();

            foreach (var expression in Expressions) result.AddRange(expression.Build());

            return result;
        }


        private List<LLVMExpression> _expressions = new List<LLVMExpression>();
    }
}
