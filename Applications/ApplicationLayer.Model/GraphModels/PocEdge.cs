using QuickGraph;
using System.Diagnostics;

namespace ApplicationLayer.Models.GraphModels
{
    /// 

    /// A simple identifiable edge.
    /// 

    [DebuggerDisplay("{Source.ID} -> {Target.ID}")]
    public class PocEdge : Edge<PocVertex>
    {
        public string ID
        {
            get;
            private set;
        }

        public PocEdge(string id, PocVertex source, PocVertex target)
            : base(source, target)
        {
            ID = id;
        }

        public static PocEdge AddNewGraphEdge(PocGraph target, PocVertex from, PocVertex to)
        {
            string edgeString = string.Format("{0}-{1} Connected", from.ID, to.ID);

            PocEdge newEdge = new PocEdge(edgeString, from, to);
            target.AddEdge(newEdge);
            return newEdge;
        }
    }
}
