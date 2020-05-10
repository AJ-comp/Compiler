using ApplicationLayer.Models.GraphModels;
using ApplicationLayer.ViewModels.Messages;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;
using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.ViewModels.DocumentTypeViewModels
{
    public class ParseTreeViewModel : DocumentViewModel
    {
        private RelayCommand<TreeSymbolVertex> _mouseDownCmd;

        #region Public Properties
        public TreeSymbol ParseTree { get; } = null;

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

        public RelayCommand<TreeSymbolVertex> MouseDownCommand
        {
            get
            {
                if (_mouseDownCmd == null)
                    _mouseDownCmd = new RelayCommand<TreeSymbolVertex>(OnMouseDown);

                return _mouseDownCmd;
            }
        }

        private void OnMouseDown(TreeSymbolVertex vertex)
        {
            Messenger.Default.Send<TreeSymbolMessage>(new TreeSymbolMessage(vertex.TreeSymbol));
        }

        public ParseTreeViewModel(TreeSymbol parseTree, string srcPath)
            : base(CommonResource.ParseTree, CommonResource.ParseTree + srcPath, CommonResource.ParseTree + srcPath)
        {
            if (parseTree is null) return;
            this.ParseTree = parseTree;

            this.CreateGraph(this.ParseTree);
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

        private PocVertex CreateNode(TreeSymbol curTree)
        {
            // Create vertex
            PocVertex curNode;
            if (curTree is TreeTerminal)
            {
                var treeTerminal = curTree as TreeTerminal;
                bool bAst = treeTerminal.Token.Kind.Meaning;
                bool bVirtual = treeTerminal.IsVirtual;
                curNode = new TreeSymbolVertex(treeTerminal);
                Graph.AddVertex(curNode);
            }
            else
            {
                var treeNonTerminal = curTree as TreeNonTerminal;
                bool bAst = (treeNonTerminal._signPost.MeaningUnit == null) ? false : true;
                bool bVirtual = treeNonTerminal.IsVirtual;
                bool bHasVirtualChild = treeNonTerminal.HasVirtualChild;
                curNode = new TreeSymbolVertex(treeNonTerminal);
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

        private void CreateGraph(TreeSymbol curTree)
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
        }

        #endregion
    }
}
