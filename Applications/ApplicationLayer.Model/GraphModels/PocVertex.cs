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

        public PocVertex(string id, bool isAst)
        {
            ID = id;
            IsAst = isAst;
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}", ID, IsAst);
        }
    }
}
