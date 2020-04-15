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
using Parse.WpfControls.SyntaxEditor.EventArgs;
using System;
using System.Collections.Generic;
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
        private int caretIndex = 0;
        private AlarmCollection alarmList = new AlarmCollection();
        private FileTreeNodeModel fileNode;
        private RelayCommand<ParsingCompletedEventArgs> parsingCompletedCommand = null;



        /********************************************************************************************
         * property section
         ********************************************************************************************/
        public Invoker MoveCaretInvoker { get; } = new Invoker();

        public string FullPath => fileNode.FullPath;
        public string Data => fileNode.Data;
        public string FileName => fileNode.FileName;

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
         * event section
         ********************************************************************************************/
        public event EventHandler<AlarmCollection> AlarmFired = null;



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
            var sementicResult = this.ParserSnippet.Parser.Grammar.SDTS.Process(parsingResult.ToAST);
            this.ParsingFailedListPreProcess(parsingResult);

            if (parsingResult.HasError == false) this.alarmList.Add(new AlarmEventArgs(string.Empty, this.FileName));
            else this.AdjustToValidAlarmList();

            this.ParsingHistory = parsingCompletedInfo.ParsingResult.ToParsingHistory;
            this.ParseTree = parsingCompletedInfo.ParsingResult.ToParseTree;

            // inform to alarm list view.
            Messenger.Default.Send<AlarmMessage>(new AlarmMessage(this));
            this.AlarmFired?.Invoke(this, parsingCompletedInfo.AlarmCollection);
        }



        /********************************************************************************************
         * private method section
         ********************************************************************************************/
        /// <summary>
        /// This function deletes useless alarm from the AlarmList.
        /// </summary>
        private void AdjustToValidAlarmList()
        {
            AlarmCollection correctList = new AlarmCollection();

            foreach (var item in this.alarmList)
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

            this.alarmList.Clear();
            foreach (var item in correctList) this.alarmList.Add(item);
        }

        private void ParsingFailedListPreProcess(ParsingResult e)
        {
            List<Tuple<int, ParsingBlock>> errorBlocks = new List<Tuple<int, ParsingBlock>>();

            Parallel.For(0, e.Count, i =>
            {
                var block = e[i];
                if (block.Token.Kind == null) return;
                if (block.ErrorInfos.Count == 0)
                {
                    block.Token.TokenCell.ValueOptionData = DrawOption.None;
                    return;
                }

                var errToken = block.Token;

                DrawOption status = DrawOption.None;
                if (errToken.TokenCell.ValueOptionData != null)
                    status = (DrawOption)errToken.TokenCell.ValueOptionData;

                if (errToken.Kind == new EndMarker())
                    status |= DrawOption.EndPointUnderline;
                else
                    status |= DrawOption.Underline;

                errToken.TokenCell.ValueOptionData = status;

                lock (errorBlocks) errorBlocks.Add(new Tuple<int, ParsingBlock>(i, block));
            });


            for (int i = 0; i < errorBlocks.Count; i++)
            {
                var tokenIndex = errorBlocks[i].Item1;
                var block = errorBlocks[i].Item2;
                var errToken = block.Token;

                // If the error fired on EndMarker then error point is last line.
                //var point = (errToken.Kind != new EndMarker()) ?
                //                    this.TextArea.GetIndexInfoFromCaretIndex(errToken.TokenCell.StartIndex) :
                //                    new System.Drawing.Point(0, this.TextArea.LineIndexes.Count - 1);

                //this.alarmList.Add(new AlarmEventArgs(string.Empty, this.FileName, tokenIndex, point.Y + 1, block));
            }
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
