using Parse.Types;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.Datas
{
    public abstract class DependencyChainVar : UseDefChainVar
    {
        public IEnumerable<DependencyChainVar> LinkedObject => _linkedObject;

        protected DependencyChainVar(IValue value) : base(value)
        {
        }

        public override void Link(DependencyChainVar toLinkObject)
        {
            _linkedObject.Add(toLinkObject);
        }

        private List<DependencyChainVar> _linkedObject = new List<DependencyChainVar>();
    }
}
