using ApplicationLayer.Models.Invokers;
using ApplicationLayer.Models.SolutionPackage;
using ApplicationLayer.Models.SolutionPackage.MiniCPackage;
using ApplicationLayer.ViewModels.DialogViewModels.OptionViewModels;
using ApplicationLayer.ViewModels.Messages;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Parse.FrontEnd;
using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.MiniC.Sdts.AstNodes;
using Parse.FrontEnd.MiniC;
using Parse.FrontEnd.ParseTree;
using Parse.FrontEnd.Support.Drawing;
using Parse.FrontEnd.Tokenize;
using Parse.WpfControls.SyntaxEditor.EventArgs;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
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
        private SourceFileTreeNodeModel _fileNode;
        private MiniCCompiler _compiler;
        private RelayCommand _saveCommand;
        private List<HighlightMapItem> _highlightMaps = new List<HighlightMapItem>();
        private RelayCommand<ParsingCompletedEventArgs> _parsingCompletedCommand = null;



        /********************************************************************************************
         * property section
         ********************************************************************************************/
        public Invoker MoveCaretInvoker { get; } = new Invoker();

        public string FullPath => _fileNode.FullPath;
        public string Data => _fileNode.Data;
        public string FileName => _fileNode.FileName;
        public string FileNameWithoutExtension => _fileNode.FileNameWithoutExtension;
        public string CurrentData { get; set; }

        public StringCollection CloseCharacters { get; } = new StringCollection();
        public Grammar Grammar { get; } = new MiniCGrammar();

        public TokenizeImpactRanges RecentTokenizeHistory { get; } = new TokenizeImpactRanges();
        public MiniCCompiler Compiler => _compiler;
        public ParseTreeSymbol ParseTree { get; private set; }
        public DataTable ParsingHistory { get; private set; }
        public IEnumerable<AstSymbol> InterLanguage { get; private set; }
        public SdtsNode Ast
        {
            get => _fileNode.Ast;
            set => _fileNode.Ast = value;
        }

        public int CaretIndex
        {
            get => _caretIndex;
            set
            {
                this._caretIndex = value;
                this.RaisePropertyChanged(nameof(CaretIndex));
            }
        }

        public RelayCommand SaveCommand
        {
            get
            {
                _saveCommand = new RelayCommand(SyncWithFile);

                return _saveCommand;
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



        public void SyncWithFile() => File.WriteAllText(_fileNode.FullPath, CurrentData);


        /********************************************************************************************
         * constructor section
         ********************************************************************************************/
        public EditorTypeViewModel(SourceFileTreeNodeModel fileNode, MiniCCompiler compiler)
            : base(fileNode?.FileName, fileNode?.FullPath, fileNode?.FullPath)
        {
            this._fileNode = fileNode;
            this._compiler = compiler;

            this.CurrentData = Data;
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
            var semanticResult = parsingCompletedInfo.SemanticResult;

            this.ParsingHistory = parsingResult.ToParsingHistory;
            this.ParseTree = parsingResult.ToParseTree;
            this.Ast = semanticResult?.SdtsRoot;


            AlarmCollection alarmCollection = GetSyntaxAlarmCollection(parsingCompletedInfo);
            alarmCollection.AddRange(GetSemanticAlarmCollection(semanticResult));
            if (alarmCollection.Count == 0) alarmCollection.Add(new AlarmEventArgs(string.Empty, this.FileName));

            // inform to alarm list view.
            Messenger.Default.Send(new AlarmMessage(this, alarmCollection));

            // Add sementic parsing information to the current FileTreeNode.
            _fileNode.Clear();
            if (semanticResult == null) return;

            if (semanticResult.SdtsRoot is MiniCNode)
            {
                var minicRoot = semanticResult.SdtsRoot as MiniCNode;
                var symbolTable = minicRoot?.SymbolTable;
                if (symbolTable == null) return;

                // Add abstract variable list information to the current FileTreeNode.
                foreach (var varRecord in symbolTable.VarTable)
                {
                    if (varRecord.DefineField.IsVirtual) continue;

                    var varTreeNode = new VarTreeNodeModel(varRecord.DefineField);
                    _fileNode.AddChildren(varTreeNode);
                }

                // Add abstract function list information to the current FileTreeNode.
                foreach (var item in symbolTable.FuncTable)
                {
                    var funcTreeNode = new FuncTreeNodeModel(item.DefineField);
                    _fileNode.AddChildren(funcTreeNode);
                }

                InterLanguage = (semanticResult.FiredException == null) ? semanticResult.AllNodes : null;
            }
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

        private AlarmCollection GetSyntaxAlarmCollection(ParsingCompletedEventArgs parsingCompletedInfo)
        {
            AlarmCollection alarmList = new AlarmCollection();

            if (parsingCompletedInfo.ParsingResult.HasError)
            {
                this.AddAlarmData(parsingCompletedInfo, alarmList);
                this.AdjustToValidAlarmList(alarmList);
            }

            return alarmList;
        }

        private AlarmCollection GetSemanticAlarmCollection(SemanticAnalysisResult semanticResult)
        {
            AlarmCollection alarmList = new AlarmCollection();

            ProjectTreeNodeModel projNode = _fileNode.ManagerTree as ProjectTreeNodeModel;

            if (semanticResult?.FiredException != null)
            {
                var alarm = ParsingErrorInfo.CreateParsingError("EX0000", semanticResult?.FiredException.Message);
                alarmList.Add(new AlarmEventArgs(projNode?.FileNameWithoutExtension, FileName, 0, 0, null, alarm));
            }

            if (semanticResult?.SdtsRoot == null) return alarmList;

            var errNodes = semanticResult?.SdtsRoot.ErrNodes;
            foreach (var item in errNodes)
            {
                foreach (var alarm in item.ConnectedErrInfoList)
                {
                    var errToken = alarm.ErrTokens[0];
                    alarmList.Add(new AlarmEventArgs(projNode?.FileNameWithoutExtension, FileName, 0, 0, errToken, alarm));
                }
            }

            return alarmList;
        }

        private void AddAlarmData(ParsingCompletedEventArgs e, AlarmCollection alarmList)
        {
            Parallel.For(0, e.ParsingResult.Count, pIndex =>
            {
                var block = e.ParsingResult[pIndex];
                if (block.ErrorInfos.Count == 0) return;

                var errToken = block.Token;
                var viewIndex = e.LexingData.GetVIndexFromPIndex(pIndex);
                var lineIndex = e.LexingData.GetLineIndex(viewIndex);

                // If the error fired on EndMarker then error point is last line.
                //var point = (errToken.Kind != new EndMarker()) ?
                //                    this.TextArea.GetIndexInfoFromCaretIndex(errToken.TokenCell.StartIndex) :
                //                    new System.Drawing.Point(0, this.TextArea.LineIndexes.Count - 1);

                ProjectTreeNodeModel projNode = _fileNode.ManagerTree as ProjectTreeNodeModel;

                lock (_lockObject)
                {
                    foreach (var errorInfo in block.ErrorInfos)
                        alarmList.Add(new AlarmEventArgs(projNode?.FileNameWithoutExtension, FileName, viewIndex, lineIndex + 1, errToken, errorInfo));
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
