using System.Diagnostics;

namespace ApplicationLayer.Models.GraphModels
{
    [DebuggerDisplay("{ID}-{IsAst}")]
    public class EbnfTreeVertex : PocVertex
    {
        public override string ID { get; }
        public bool IsAst { get; private set; }

        public EbnfTreeVertex(string id, bool isAst)
        {
            ID = id;
            IsAst = isAst;
        }

        public override string ToString() => string.Format("{0}-{1}", ID, IsAst);
    }
}
