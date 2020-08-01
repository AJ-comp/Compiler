using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions
{
    public class LLVMRootExpression : LLVMExpression
    {
        public List<LLVMFirstLayerExpression> FirstLayers { get; } = new List<LLVMFirstLayerExpression>();

        public LLVMRootExpression() : base(new LLVMSSATable())
        {
        }

        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> result = new List<Instruction>();

            foreach (var item in FirstLayers)
                result.AddRange(item.Build());

            return result;
        }
    }
}
