using Parse.Types;

namespace Parse.MiddleEnd.IR.Datas
{
    public class RootChainVar : UseDefChainVar
    {
        public DependencyChainVar LinkedObject { get; private set; }

        public static IRVar EmptyRootChainVar => new RootChainVar();

        public override DType TypeName { get; }
        public override int Block { get; set; }
        public override int Offset { get; set; }
        public override int Length => 0;

        public RootChainVar(IRVar var) : base(var.PointerLevel)
        {
            TypeName = var.TypeName;
            Name = var.Name;
        }

        public override void Link(DependencyChainVar toLinkObject)
        {
            LinkedObject = toLinkObject;
        }

        /// <summary>
        /// This function returns new IRVar type that Name property modified to nameToChange.
        /// This function used to create RootChainVar to save param value.
        /// </summary>
        /// <param name="var"></param>
        /// <param name="nameToChange"></param>
        /// <returns></returns>
        public static IRVar Copy(IRVar var, string nameToChange)
        {
            var result = new RootChainVar(var);
            result.Name = nameToChange;

            return result;
        }

        public override string ToString()
        {
            return string.Format("TypeName : {0}, Name : {1}, PointerLevel : {2}, Offset : {3}, Length {4}",
                                            TypeName, Name, PointerLevel, Offset, Length);
        }




        private RootChainVar() : base(0)
        {
            TypeName = DType.Int;
        }
    }
}
