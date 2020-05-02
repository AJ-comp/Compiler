using System.Diagnostics;

namespace ApplicationLayer.Models.GraphModels
{
    /// 

    /// A simple identifiable vertex.
    /// 

    [DebuggerDisplay("{ID}-{IsAst}")]
    public class PocVertex
    {
        public string ID { get; private set; }
        public bool IsAst { get; private set; }
        public bool IsVirtual { get; private set; }
        public bool HasVirtualChild { get; private set; }

        public PocVertex(string id, bool isAst, bool isVirtual = false, bool hasVirtualChild = false)
        {
            ID = id;
            IsAst = isAst;
            IsVirtual = isVirtual;
            HasVirtualChild = hasVirtualChild;
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}", ID, IsAst);
        }
    }
}
