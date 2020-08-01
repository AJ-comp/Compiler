using Parse.Types;
using Parse.Types.ConstantTypes;

namespace Parse.MiddleEnd.IR.Datas
{
    public class RootChainVar : UseDefChainVar
    {
        public DependencyChainVar LinkedObject { get; private set; }

        public override DType TypeName { get; }
        public override string Name { get; }
        public override int Block => throw new System.NotImplementedException();
        public override int Offset { get => throw new System.NotImplementedException(); protected set => throw new System.NotImplementedException(); }
        public override int Length => throw new System.NotImplementedException();

        public RootChainVar(IRVar var) : base(var)
        {
            TypeName = var.TypeName;
            Name = var.Name;
        }

        public override void Link(DependencyChainVar toLinkObject)
        {
            LinkedObject = toLinkObject;
        }

        public override IConstant Assign(IValue operand)
        {
            throw new System.NotImplementedException();
        }
    }
}
