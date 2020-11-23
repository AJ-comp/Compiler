using System.Collections.Generic;
using System.Linq;

namespace Parse.MiddleEnd.IR.Datas
{
    public abstract class DependencyChainVar : UseDefChainVar
    {
        public IEnumerable<DependencyChainVar> LinkedObject => _linkedObject;


        public string SummaryString() => base.ToString();

        protected DependencyChainVar(uint pointerLevel) : base(pointerLevel)
        {
        }

        public override void Link(DependencyChainVar toLinkObject)
        {
            _linkedObject.Add(toLinkObject);
        }

        public override string ToString()
        {
            var result = SummaryString();

            if (LinkedObject.Count() == 0) return result;

            result += " -> [";
            foreach (var item in LinkedObject)
                result += item.SummaryString() + " ";

            result += "]";
            return result;
        }

        private List<DependencyChainVar> _linkedObject = new List<DependencyChainVar>();
    }
}
