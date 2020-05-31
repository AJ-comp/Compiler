using ApplicationLayer.Common.Utilities;
using ApplicationLayer.Models.Invokers;
using ApplicationLayer.Models.SolutionPackage;
using ApplicationLayer.Models.SolutionPackage.MiniCPackage;
using ApplicationLayer.ViewModels.DialogViewModels.OptionViewModels;
using ApplicationLayer.ViewModels.Messages;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Parse.FrontEnd;
using Parse.FrontEnd.Ast;
using Parse.FrontEnd.DrawingSupport;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.VarDataFormat;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.Logical;
using Parse.FrontEnd.ParseTree;
using Parse.Tokenize;
using Parse.WpfControls.SyntaxEditor.EventArgs;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ApplicationLayer.ViewModels.DocumentTypeViewModels
{
    public class EditorTypeViewModel : DocumentViewModel
    {
        /********************************************************************************************
         * private field section
         ********************************************************************************************/
        private object _lockObject = new object();
        private int _caretIndex = 0;
        private FileTreeNodeModel _fileNode;
        private ParserSnippet _parserSnippet;
        private List<HighlightMapItem> _highlightMaps = new List<HighlightMapItem>();
        private RelayCommand<ParsingCompletedEventArgs> _parsingCompletedCommand = null;



        /********************************************************************************************
         * property section
         ********************************************************************************************/
        public Invoker MoveCaretInvoker { get; } = new Invoker();

        public string FullPath => _fileNode.FullPath;
        public string Data => _fileNode.Data;
        public string FileName => _fileNode.FileName;
        public StringCollection CloseCharacters { get; } = new StringCollection();
        public Grammar Grammar { get; } = new MiniCGrammar();

        public TokenizeImpactRanges RecentTokenizeHistory { get; } = new TokenizeImpactRanges();
        public ParserSnippet ParserSnippet => _parserSnippet;
        public ParseTreeSymbol ParseTree { get; private set; }
        public AstSymbol Ast { get; private set; }
        public DataTable ParsingHistory { get; private set; }
        public IReadOnlyList<AstSymbol> InterLanguage { get; private set; }

        public int CaretIndex
        {
            get => _caretIndex;
            set
            {
                this._caretIndex = value;
                this.RaisePropertyChanged(nameof(CaretIndex));
            }
        }

        public FontsAndColorsViewModel FontsAndColorsVM => FontsAndColorsViewModel.Instance;



        /********************************************************************************************
         * command property section
         ********************************************************************************************/
        public RelayCommand<ParsingCompletedEventArgs> ParsingCompletedCommand
        {
            get
            {
                if (this._parsingCompletedCommand == null)
                    this._parsingCompletedCommand = new RelayCommand<ParsingCompletedEventArgs>(this.OnParsingCompleted);

                return this._parsingCompletedCommand;
            }
        }



        /********************************************************************************************
         * constructor section
         ********************************************************************************************/
        public EditorTypeViewModel(FileTreeNodeModel fileNode) : base(fileNode?.FileName, fileNode?.FullPath, fileNode?.FullPath)
        {
            this._fileNode = fileNode;
            this._parserSnippet = ParserFactory.Instance.GetParser(ParserFactory.ParserKind.SLR_Parser, Grammar).NewParserSnippet();

            this.CloseCharacters.Add("{");
            this.CloseCharacters.Add("}");
            this.CloseCharacters.Add("(");
            this.CloseCharacters.Add(")");
            this.CloseCharacters.Add("\t");
            this.CloseCharacters.Add(" ");
            this.CloseCharacters.Add(Environment.NewLine);
        }



        /********************************************************************************************
         * event handler section
         ********************************************************************************************/
        private void OnParsingCompleted(ParsingCompletedEventArgs parsingCompletedInfo)
        {
            var parsingResult = parsingCompletedInfo.ParsingResult;

            this.ParsingHistory = parsingCompletedInfo.ParsingResult.ToParsingHistory;
            this.ParseTree = parsingCompletedInfo.ParsingResult.ToParseTree;
            this.Ast = parsingCompletedInfo.RootAst;

            AlarmCollection alarmCollection = GetSyntaxAlarmCollection(parsingResult);
            alarmCollection.AddRange(GetSemanticAlarmCollection(parsingCompletedInfo.AllNodes, parsingCompletedInfo.FiredException));
            if (alarmCollection.Count == 0) alarmCollection.Add(new AlarmEventArgs(string.Empty, this.FileName));

            // inform to alarm list view.
            Messenger.Default.Send<AlarmMessage>(new AlarmMessage(this, alarmCollection));

            // Add sementic parsing information to the current FileTreeNode.
            _fileNode.Clear();

            var astRoot = parsingCompletedInfo.RootAst as AstNonTerminal;
            var grammarSymbolTable = astRoot?.ConnectedSymbolTable as MiniCSymbolTable;
            if (grammarSymbolTable == null) return;

            // Add abstract variable list information to the current FileTreeNode.
            foreach(var item in grammarSymbolTable.VarDataList)
            {
                if (item is VirtualVarData) continue;

                var cItem = item as RealVarData;

                var varTreeNode = new VarTreeNodeModel(cItem.DclData);
                _fileNode.AddChildren(varTreeNode);
            }

            // Add abstract function list information to the current FileTreeNode.
            foreach(var item in grammarSymbolTable.FuncDataList)
            {
                var funcTreeNode = new FuncTreeNodeModel(item);
                _fileNode.AddChildren(funcTreeNode);
            }

            InterLanguage = (parsingCompletedInfo.FiredException == null) ? parsingCompletedInfo.AllNodes : null;
        }



        /********************************************************************************************
         * private method section
         ********************************************************************************************/
        /// <summary>
        /// This function deletes useless alarm from the AlarmList.
        /// </summary>
        private void AdjustToValidAlarmList(AlarmCollection alarmList)
        {
            AlarmCollection correctList = new AlarmCollection();

            foreach (var item in alarmList)
            {
                //                if (this.TextArea.Tokens[tokenIndex] == item.ParsingFailedArgs.InputValue.TokenCell) correctList.Add(item);
                correctList.Add(item);
            }

            /*
            var correctList = this.alarmList.Where(x =>
            {
                int tokenIndex = x.ParsingFailedArgs.ErrorIndex;
                return (this.TextArea.Tokens[tokenIndex] == x.ParsingFailedArgs.InputValue.TokenCell);
            });
            */

            alarmList.Clear();
            foreach (var item in correctList) alarmList.Add(item);
        }

        private AlarmCollection GetSyntaxAlarmCollection(ParsingResult parsingResult)
        {
            AlarmCollection alarmList = new AlarmCollection();
            if (parsingResult.HasError)
            {
                this.AddAlarmData(parsingResult, alarmList);
                this.AdjustToValidAlarmList(alarmList);
            }

            return alarmList;
        }

        private AlarmCollection GetSemanticAlarmCollection(IReadOnlyList<AstSymbol> astNodes, Exception e)
        {
            AlarmCollection alarmList = new AlarmCollection();

            ProjectTreeNodeModel projNode = _fileNode.ManagerTree as ProjectTreeNodeModel;

            if(e != null)
            {
                var alarm = ParsingErrorInfo.CreateParsingError("EX0000", e.Message);
                alarmList.Add(new AlarmEventArgs(projNode?.FileNameWithoutExtension, FileName, 0, 0, null, alarm));
            }

            if (astNodes == null) return alarmList;

            foreach (var item in astNodes)
            {
                foreach(var alarm in item.ConnectedErrInfoList)
                {
                    var errToken = alarm.ErrorTokens[0];
                    alarmList.Add(new AlarmEventArgs(projNode?.FileNameWithoutExtension, FileName, 0, 0, errToken, alarm));
                }
            }

            return alarmList;
        }

        private void AddAlarmData(ParsingResult e, AlarmCollection alarmList)
        {
            int lineIndex = 0;
            Parallel.For(0, e.Count, tokenIndex => 
            {
                var block = e[tokenIndex];
                if (block.Token.Input == "\n") lineIndex++;                // this may need to use a lock object.
                if (block.ErrorInfos.Count == 0) return;

                var errToken = block.Token;

                // If the error fired on EndMarker then error point is last line.
                //var point = (errToken.Kind != new EndMarker()) ?
                //                    this.TextArea.GetIndexInfoFromCaretIndex(errToken.TokenCell.StartIndex) :
                //                    new System.Drawing.Point(0, this.TextArea.LineIndexes.Count - 1);

                ProjectTreeNodeModel projNode = _fileNode.ManagerTree as ProjectTreeNodeModel;

                lock(_lockObject)
                {
                    foreach(var errorInfo in block.ErrorInfos)
                        alarmList.Add(new AlarmEventArgs(projNode?.FileNameWithoutExtension, FileName, tokenIndex, lineIndex + 1, errToken, errorInfo));
                }
            });
        }



        /********************************************************************************************
         * override method section
         ********************************************************************************************/
        public override bool Equals(object obj)
        {
            var model = obj as EditorTypeViewModel;
            return model != null &&
                   FullPath == model.FullPath;
        }

        public override int GetHashCode()
        {
            return 2018552787 + EqualityComparer<string>.Default.GetHashCode(FullPath);
        }
    }
}
