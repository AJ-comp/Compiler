using ApplicationLayer.Models.GraphModels;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Threading.Tasks;

namespace ApplicationLayer.ViewModels.SubViewModels
{
    public class LRParserViewModel : ParserViewModel
    {
        private string grammarLayoutAlgorithmType;
        private string canonicalLayoutAlgorithmType;
        private NonTerminal selectedNonTerminal;
        private int selectedCanonical;
        private PocGraph ebnfGraph;
        private PocGraph canonicalGraph;
        private List<string> layoutAlgorithmTypes = new List<string>();

        public LRParser LRParser { get; private set; }
        public List<string> LayoutAlgorithmTypes
        {
            get { return layoutAlgorithmTypes; }
        }

        #region The property related to Grammar tab
        public PocGraph EbnfGraph
        {
            get => ebnfGraph;
            private set
            {
                ebnfGraph = value;
                RaisePropertyChanged(nameof(EbnfGraph));
            }
        }
        public List<NonTerminal> NonTerminals { get; } = new List<NonTerminal>();
        public NonTerminal SelectedNonTerminal
        {
            get => selectedNonTerminal;
            set
            {
                selectedNonTerminal = value;
                CreateEbnfGraph();
                RaisePropertyChanged(nameof(SelectedNonTerminal));
            }
        }
        public string GrammarLayoutAlgorithmType
        {
            get { return grammarLayoutAlgorithmType; }
            set
            {
                grammarLayoutAlgorithmType = value;
                RaisePropertyChanged(nameof(GrammarLayoutAlgorithmType));
            }
        }
        #endregion

        #region The property related to Canonical tab
        public PocGraph CanonicalGraph
        {
            get => canonicalGraph;
            private set
            {
                canonicalGraph = value;
                RaisePropertyChanged(nameof(CanonicalGraph));
            }
        }

        public List<string> Canonicals { get; } = new List<string>();

        public int SelectedCanonical
        {
            get => selectedCanonical;
            set
            {
                selectedCanonical = value;
                CreateCanonicalGraph();
                RaisePropertyChanged(nameof(SelectedCanonical));
            }
        }

        public string CanonicalLayoutAlgorithmType
        {
            get { return canonicalLayoutAlgorithmType; }
            set
            {
                canonicalLayoutAlgorithmType = value;
                RaisePropertyChanged(nameof(CanonicalLayoutAlgorithmType));
            }
        }


        private string canonicalString;
        public string CanonicalString
        {
            get => canonicalString;
            set
            {
                canonicalString = value;
                RaisePropertyChanged(nameof(CanonicalString));
            }
        }

        #endregion

        public DataView ParsingTable => LRParser.ParsingTable.ToTableFormat.DefaultView;
        public List<string> ToolTipDatas
        {
            get
            {
                List<string> result = new List<string>();

                for (int i = 0; i <= this.LRParser.C0.MaxIxIndex; i++)
                {
                    CanonicalItemSet canonical = this.LRParser.C0.GetStatusFromIxIndex(i);
                    result.Add(canonical.ToLineString());
                }

                return result;
            }
        }

        public LRParserViewModel(LRParser lrParser)
        {
            this.LRParser = lrParser;

            //Add Layout Algorithm Types
            layoutAlgorithmTypes.Add("BoundedFR");
            layoutAlgorithmTypes.Add("Circular");
            layoutAlgorithmTypes.Add("CompoundFDP");
            layoutAlgorithmTypes.Add("EfficientSugiyama");
            layoutAlgorithmTypes.Add("FR");
            layoutAlgorithmTypes.Add("ISOM");
            layoutAlgorithmTypes.Add("KK");
            layoutAlgorithmTypes.Add("LinLog");
            layoutAlgorithmTypes.Add("Tree");

            //Pick a default Layout Algorithm Type
            GrammarLayoutAlgorithmType = "Tree";
            CanonicalLayoutAlgorithmType = "ISOM";

            if (this.LRParser == null) return;

            foreach (var item in this.LRParser.Grammar.NonTerminalMultiples)
                NonTerminals.Add(item);

            for (int i = 0; i <= this.LRParser.C0.MaxIxIndex; i++) 
                Canonicals.Add(string.Format("I{0}", i));
        }

        private void CreateEbnfNode(NonTerminal curNT)
        {
            // Create vertex
            PocVertex curNode = new PocVertex(curNT.ToString(), true);
            EbnfGraph.AddVertex(curNode);

            foreach (var singleExpr in curNT)
            {
                string strToAdd = string.Empty;

                foreach (var child in singleExpr) strToAdd += child.ToString() + " ";

                PocVertex childNode = new PocVertex(strToAdd, true);
                EbnfGraph.AddVertex(childNode);
                PocEdge.AddNewGraphEdge(EbnfGraph, curNode, childNode);
            }
        }

        private void CreateEbnfGraph()
        {
            EbnfGraph = new PocGraph(true);

            if (SelectedNonTerminal == null) return;

            CreateEbnfNode(SelectedNonTerminal);
        }

        private string GetHostCanonicalString(int index)
        {
            string result = string.Empty;

            Parallel.ForEach(LRParser.C0, (data, loopOption) =>
            {
                if (data.Value.Item1 != index) return;

                result = string.Format("I{0} : ", data.Value.Item1) + Environment.NewLine;
                result += data.Value.Item2.ToLineString();

                loopOption.Stop();
            });

            return result;
        }

        private StringCollection GetMovableCanonicalStringCollection(int index)
        {
            StringCollection result = new StringCollection();

            Parallel.ForEach(LRParser.C0, (data, loopOption) =>
            {
                if (data.Key.Item1 != index) return;

                string cx = string.Format("Goto(I{0},{1})", data.Key.Item1, data.Key.Item2) + Environment.NewLine;
                cx += string.Format("I{0} : ", data.Value.Item1) + Environment.NewLine;
                cx += data.Value.Item2.ToLineString();

                result.Add(cx);
            });

            return result;
        }

        private void CreateCanonicalGraph()
        {
            CanonicalGraph = new PocGraph(true);

            var host = GetHostCanonicalString(selectedCanonical);
            var movableList = GetMovableCanonicalStringCollection(selectedCanonical);

            if (host.Length == 0) return;

            PocVertex hostVertex = new PocVertex(host, true);
            CanonicalGraph.AddVertex(hostVertex);
            foreach(var item in movableList)
            {
                PocVertex movableVertex = new PocVertex(item, true);
                CanonicalGraph.AddVertex(movableVertex);
                PocEdge.AddNewGraphEdge(CanonicalGraph, hostVertex, movableVertex);
            }
        }
    }
}
