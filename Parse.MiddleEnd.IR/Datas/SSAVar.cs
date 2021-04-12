using Parse.MiddleEnd.IR.Interfaces;
using Parse.Types;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Parse.MiddleEnd.IR.Datas
{
    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public abstract class SSAVar : ISSAForm, IRDeclareVar
    {
        public IEnumerable<SSAVar> LinkedObject => _linkedObject;

        public abstract StdType TypeKind { get; }
        public string Name { get; protected set; }
        public string PartyName { get; set; }
        public int Block { get; set; }
        public abstract int Offset { get; set; }
        public int Length { get; protected set; }
        public uint PointerLevel { get; set; }
        public IRExpr InitialExpr { get; set; }

        public void Link(SSAVar toLinkObject)
        {
            _linkedObject.Add(toLinkObject);
        }

        public virtual string DebuggerDisplay
        {
            get
            {
                string result = IRFormatter.ToDebugFormat(this);

                if (LinkedObject.Count() == 0) return result;

                result += " -> ";
                foreach (var item in LinkedObject)
                    result += item.DebuggerDisplay;

                return result;
            }
        }

        private List<SSAVar> _linkedObject = new List<SSAVar>();
    }
}
