using ApplicationLayer.Models.GraphModels;
using ApplicationLayer.ViewModels.Messages;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Parse.FrontEnd.Ast;
using Parse.FrontEnd.ParseTree;
using System;
using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.ViewModels.DocumentTypeViewModels
{
    public class ParseTreeViewModel : DocumentViewModel
    {
        private bool _selectedParseTree;
        private bool _selectedAst;
        private RelayCommand<object> _mouseDownCmd;

        #region Public Properties
        public bool SelectedParseTree
        {
            get => _selectedParseTree;
            set
            {
                _selectedParseTree = value;
                bool visible = _selectedParseTree ? true : false;

                if (AstGraphVM != null) AstGraphVM.IsVisible = !visible;
                if (ParseTreeGraphVM != null) ParseTreeGraphVM.IsVisible = visible;

                RaisePropertyChanged(nameof(SelectedParseTree));
            }
        }

        public bool SelectedAst
        {
            get => _selectedAst;
            set
            {
                _selectedAst = value;

                RaisePropertyChanged(nameof(SelectedAst));
            }
        }

        public GraphLayoutViewModel ParseTreeGraphVM { get; private set; }
        public GraphLayoutViewModel AstGraphVM { get; private set; }

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

            PocGraph parseTreeGraph = new PocGraph(true);
            CreateNode(parseTreeGraph, parseTree);
            ParseTreeGraphVM = new GraphLayoutViewModel(parseTreeGraph);

            if (ast is null) return;

            PocGraph astGraph = new PocGraph(true);
            CreateNode(astGraph, ast);
            AstGraphVM = new GraphLayoutViewModel(astGraph);

            SelectedParseTree = true;
        }

        #region Private Methods
        private PocEdge AddNewGraphEdge(PocGraph graph, PocVertex from, PocVertex to)
        {
            string edgeString = FormattableString.Invariant($"{from.ID}-{to.ID} Connected");
//            string edgeString = string.Format("{0}-{1} Connected", from.ID, to.ID);

            PocEdge newEdge = new PocEdge(edgeString, from, to);
            graph.AddEdge(newEdge);
            return newEdge;
        }

        private PocVertex CreateNode(PocGraph graph, ParseTreeSymbol curTree)
        {
            // Create vertex
            PocVertex curNode = null;
            if (curTree == null) return curNode;

            if (curTree is ParseTreeTerminal)
            {
                var treeTerminal = curTree as ParseTreeTerminal;
                curNode = new ParseTreeSymbolVertex(treeTerminal);
                graph.AddVertex(curNode);
            }
            else
            {
                var treeNonTerminal = curTree as ParseTreeNonTerminal;
                curNode = new ParseTreeSymbolVertex(treeNonTerminal);
                graph.AddVertex(curNode);

                foreach (var childTree in treeNonTerminal.Items)
                {
                    //add some edges to the graph
                    var childNode = CreateNode(graph, childTree);
                    AddNewGraphEdge(graph, curNode, childNode);
                }
            }

            return curNode;
        }

        private PocVertex CreateNode(PocGraph graph, AstSymbol curTree)
        {
            // Create vertex
            PocVertex curNode = null;
            if (curTree == null) return curNode;

            if (curTree is AstTerminal)
            {
                var treeTerminal = curTree as AstTerminal;
                curNode = new AstSymbolVertex(treeTerminal);
                graph.AddVertex(curNode);
            }
            else
            {
                var treeNonTerminal = curTree as AstNonTerminal;
                curNode = new AstSymbolVertex(treeNonTerminal);
                graph.AddVertex(curNode);

                foreach (var childTree in treeNonTerminal.Items)
                {
                    //add some edges to the graph
                    var childNode = CreateNode(graph, childTree);
                    AddNewGraphEdge(graph, curNode, childNode);
                }
            }

            return curNode;
        }

        #endregion
    }
}
