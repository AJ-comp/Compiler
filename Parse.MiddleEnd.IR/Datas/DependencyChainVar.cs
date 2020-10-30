using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.Datas
{
    public abstract class DependencyChainVar : UseDefChainVar
    {
        public IEnumerable<DependencyChainVar> LinkedObject => _linkedObject;

        protected DependencyChainVar(uint pointerLevel) : base(pointerLevel)
        {
        }

        public override void Link(DependencyChainVar toLinkObject)
        {
            _linkedObject.Add(toLinkObject);
        }

        private List<DependencyChainVar> _linkedObject = new List<DependencyChainVar>();
    }
}
