using ApplicationLayer.Models.GraphModels;
using ApplicationLayer.ViewModels.Messages;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Parse.FrontEnd.Ast;
using Parse.FrontEnd.ParseTree;
using System;
using System.Collections.Generic;
using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.ViewModels.DocumentTypeViewModels
{
    public class ParseTreeViewModel : DocumentViewModel
    {
        private bool _selectedParseTreeViewMode;
        private bool _selectedAstViewMode;
        private RelayCommand<object> _mouseDownCmd;

        #region Public Properties
        public ParseTreeSymbol ParseTree { get; } = null;
        public AstSymbol Ast { get; } = null;

        public bool SelectedParseTreeViewMode
        {
            get => _selectedParseTreeViewMode;
            set
            {
                _selectedParseTreeViewMode = value;
                Graph = new PocGraph(true);

                CreateNode(ParseTree);
                RaisePropertyChanged(nameof(SelectedParseTreeViewMode));
            }
        }

        public bool SelectedAstViewMode
        {
            get => _selectedAstViewMode;
            set
            {
                _selectedAstViewMode = value;
                Graph = new PocGraph(true);

                CreateNode(Ast);
                RaisePropertyChanged(nameof(SelectedAstViewMode));
            }
        }

        public List<string> LayoutAlgorithmTypes
        {
            get { return layoutAlgorithmTypes; }
        }

        public string LayoutAlgorithmType
        {
            get { return layoutAlgorithmType; }
            set
            {
                layoutAlgorithmType = value;
                this.RaisePropertyChanged(nameof(LayoutAlgorithmType));
            }
        }

        public PocGraph Graph
        {
            get { return graph; }
            set
            {
                graph = value;
                this.RaisePropertyChanged(nameof(Graph));
            }
        }
        #endregion

        public RelayCommand<object> MouseDownCommand
        {
            get
            {
                if (_mouseDownCmd == null)
                    _mouseDownCmd = new RelayCommand<object>(OnMouseDown);

                return _mouseDownCmd;
            }
        }

        private void OnMouseDown(object param)
        {
            if(param is AstSymbolVertex)
            {
                var vertex = param as AstSymbolVertex;
                Messenger.Default.Send<TreeSymbolMessage>(new TreeSymbolMessage(vertex.TreeSymbol));
            }
        }

        public ParseTreeViewModel(ParseTreeSymbol parseTree, AstSymbol ast, string srcPath)
            : base(CommonResource.ParseTree, CommonResource.ParseTree + srcPath, CommonResource.ParseTree + srcPath)
        {
            if (parseTree is null) return;
            this.ParseTree = parseTree;
            this.Ast = ast;

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
            LayoutAlgorithmType = "Tree";

            SelectedParseTreeViewMode = true;
        }



        #region Data
        private string layoutAlgorithmType;
        private PocGraph graph;
        private List<string> layoutAlgorithmTypes = new List<string>();
        #endregion

        #region Ctor
        #endregion

        #region Private Methods
        private PocEdge AddNewGraphEdge(PocVertex from, PocVertex to)
        {
            string edgeString = FormattableString.Invariant($"{from.ID}-{to.ID} Connected");
//            string edgeString = string.Format("{0}-{1} Connected", from.ID, to.ID);

            PocEdge newEdge = new PocEdge(edgeString, from, to);
            Graph.AddEdge(newEdge);
            return newEdge;
        }

        private PocVertex CreateNode(ParseTreeSymbol curTree)
        {
            // Create vertex
            PocVertex curNode;
            if (curTree is ParseTreeTerminal)
            {
                var treeTerminal = curTree as ParseTreeTerminal;
                bool bAst = treeTerminal.Token.Kind.Meaning;
                bool bVirtual = treeTerminal.IsVirtual;
                curNode = new ParseTreeSymbolVertex(treeTerminal);
                Graph.AddVertex(curNode);
            }
            else
            {
                var treeNonTerminal = curTree as ParseTreeNonTerminal;
                bool bAst = (treeNonTerminal.SignPost.MeaningUnit == null) ? false : true;
                bool bVirtual = treeNonTerminal.IsVirtual;
                bool bHasVirtualChild = treeNonTerminal.HasVirtualChild;
                curNode = new ParseTreeSymbolVertex(treeNonTerminal);
                Graph.AddVertex(curNode);

                foreach (var childTree in treeNonTerminal.Items)
                {
                    //add some edges to the graph
                    var childNode = CreateNode(childTree);
                    AddNewGraphEdge(curNode, childNode);
                }
            }

            return curNode;
        }

        private PocVertex CreateNode(AstSymbol curTree)
        {
            // Create vertex
            PocVertex curNode;
            if (curTree is AstTerminal)
            {
                var treeTerminal = curTree as AstTerminal;
                curNode = new AstSymbolVertex(treeTerminal);
                Graph.AddVertex(curNode);
            }
            else
            {
                var treeNonTerminal = curTree as AstNonTerminal;
                curNode = new AstSymbolVertex(treeNonTerminal);
                Graph.AddVertex(curNode);

                foreach (var childTree in treeNonTerminal.Items)
                {
                    //add some edges to the graph
                    var childNode = CreateNode(childTree);
                    AddNewGraphEdge(curNode, childNode);
                }
            }

            return curNode;
        }

        private void CreateGraph(ParseTreeSymbol curTree)
        {
            Graph = new PocGraph(true);

            CreateNode(curTree);
            /*
            string edgeString = string.Format("{0}-{1} Connected", existingVertices[0].ID, existingVertices[0].ID);
            Graph.AddEdge(new PocEdge(edgeString, existingVertices[0], existingVertices[1]));
            Graph.AddEdge(new PocEdge(edgeString, existingVertices[0], existingVertices[1]));
            Graph.AddEdge(new PocEdge(edgeString, existingVertices[0], existingVertices[1]));
            Graph.AddEdge(new PocEdge(edgeString, existingVertices[0], existingVertices[1]));
            */
        }

        #endregion
    }
}
