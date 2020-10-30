using Parse.Types;

namespace Parse.MiddleEnd.IR.Datas
{
    public class RootChainVar : UseDefChainVar
    {
        public DependencyChainVar LinkedObject { get; private set; }

        public override DType TypeName { get; }
        public override string Name { get; }
        public override int Block { get; set; }
        public override int Offset { get; set; }
        public override int Length => throw new System.NotImplementedException();

        public RootChainVar(IRVar var) : base(var.PointerLevel)
        {
            TypeName = var.TypeName;
            Name = var.Name;
        }

        public override void Link(DependencyChainVar toLinkObject)
        {
            LinkedObject = toLinkObject;
        }

        public override string ToString()
        {
            return string.Format("TypeName : {0}, Name : {1}, PointerLevel : {2}, Offset : {3}, Length {4}",
                                            TypeName, Name, PointerLevel, Offset, Length);
        }
    }
}
