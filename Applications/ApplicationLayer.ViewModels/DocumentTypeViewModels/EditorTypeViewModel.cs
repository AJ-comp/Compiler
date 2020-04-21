using ApplicationLayer.Common.Utilities;
using ApplicationLayer.Models.Invokers;
using ApplicationLayer.Models.SolutionPackage;
using ApplicationLayer.ViewModels.Messages;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Parse;
using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.Logical;
using Parse.FrontEnd.RegularGrammar;
using Parse.Tokenize;
using Parse.WpfControls.SyntaxEditor.EventArgs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ApplicationLayer.ViewModels.DocumentTypeViewModels
{
    public class EditorTypeViewModel : DocumentViewModel
    {
        /********************************************************************************************
         * private field section
         ********************************************************************************************/
        private object lockObject = new object();
        private int caretIndex = 0;
        private FileTreeNodeModel fileNode;
        private RelayCommand<ParsingCompletedEventArgs> parsingCompletedCommand = null;



        /********************************************************************************************
         * property section
         ********************************************************************************************/
        public Invoker MoveCaretInvoker { get; } = new Invoker();

        public string FullPath => fileNode.FullPath;
        public string Data => fileNode.Data;
        public string FileName => fileNode.FileName;

        public TokenizeImpactRanges RecentTokenizeHistory { get; } = new TokenizeImpactRanges();
        public ParserSnippet ParserSnippet { get; } = ParserFactory.Instance.GetParser(ParserFactory.ParserKind.SLR_Parser, new MiniCGrammar()).NewParserSnippet();
        public TreeSymbol ParseTree { get; private set; }
        public DataTable ParsingHistory { get; private set; }

        public int CaretIndex
        {
            get => caretIndex;
            set
            {
                this.caretIndex = value;
                this.RaisePropertyChanged(nameof(CaretIndex));
            }
        }



        /********************************************************************************************
         * command property section
         ********************************************************************************************/
        public RelayCommand<ParsingCompletedEventArgs> ParsingCompletedCommand
        {
            get
            {
                if (this.parsingCompletedCommand == null)
                    this.parsingCompletedCommand = new RelayCommand<ParsingCompletedEventArgs>(this.OnParsingCompleted);

                return this.parsingCompletedCommand;
            }
        }



        /********************************************************************************************
         * constructor section
         ********************************************************************************************/
        public EditorTypeViewModel(FileTreeNodeModel fileNode) : base(fileNode.FileName, fileNode.FullPath, fileNode.FullPath)
        {
            this.fileNode = fileNode;
        }



        /********************************************************************************************
         * event handler section
         ********************************************************************************************/
        private void OnParsingCompleted(ParsingCompletedEventArgs parsingCompletedInfo)
        {
            var parsingResult = parsingCompletedInfo.ParsingResult;

            AlarmCollection alarmList = new AlarmCollection();
            if (parsingResult.HasError == false) alarmList.Add(new AlarmEventArgs(string.Empty, this.FileName));
            else
            {
                this.AddAlarmData(parsingResult, alarmList);
                this.AdjustToValidAlarmList(alarmList);
            }

            this.ParsingHistory = parsingCompletedInfo.ParsingResult.ToParsingHistory;
            this.ParseTree = parsingCompletedInfo.ParsingResult.ToParseTree;

            // inform to alarm list view.
            Messenger.Default.Send<AlarmMessage>(new AlarmMessage(this, alarmList));
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
                int tokenIndex = item.TokenIndex;
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

                ProjectTreeNodeModel projNode = fileNode.ManagerTree as ProjectTreeNodeModel;

                lock(lockObject)
                    alarmList.Add(new AlarmEventArgs(projNode?.FileNameWithoutExtension, FileName, tokenIndex, lineIndex + 1, errToken, block.ErrorInfos));
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
