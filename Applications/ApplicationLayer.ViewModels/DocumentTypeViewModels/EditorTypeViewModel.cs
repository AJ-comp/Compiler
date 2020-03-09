using ApplicationLayer.Common.Utilities;
using ApplicationLayer.Models.Invokers;
using GalaSoft.MvvmLight.Command;
using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.Logical;
using Parse.WpfControls.SyntaxEditor.EventArgs;
using System;
using System.Collections.Generic;
using System.IO;

namespace ApplicationLayer.ViewModels.DocumentTypeViewModels
{
    public class EditorTypeViewModel : DocumentViewModel
    {
        public Invoker MoveCaretInvoker { get; } = new Invoker();
        public event EventHandler<AlarmCollection> AlarmFired = null;

        public string FullPath { get; } = string.Empty;
        public string Data { get; } = string.Empty;

        public ParserSnippet ParserSnippet { get; } = ParserFactory.Instance.GetParser(ParserFactory.ParserKind.SLR_Parser, new MiniCGrammar()).NewParserSnippet();
        public IReadOnlyList<TreeSymbol> ParseTree { get; private set; }
        public ParsingHistory ParsingHistory { get; private set; }

        private int caretIndex = 0;
        public int CaretIndex
        {
            get => caretIndex;
            set
            {
                this.caretIndex = value;
                this.RaisePropertyChanged(nameof(CaretIndex));
            }
        }

        public EditorTypeViewModel() : base("New Document")
        {
        }

        public EditorTypeViewModel(string fullPath) : base(Path.GetFileName(fullPath), fullPath)
        {
            this.FullPath = fullPath;
        }

        public EditorTypeViewModel(string fullPath, string data) : this(fullPath)
        {
            this.Data = data;
        }

        private RelayCommand<AlarmCollection> alarmFiredCommand = null;
        public RelayCommand<AlarmCollection> AlarmFiredCommand
        {
            get
            {
                if (this.alarmFiredCommand == null)
                    this.alarmFiredCommand = new RelayCommand<AlarmCollection>(this.OnAlarmFired);

                return this.alarmFiredCommand;
            }
        }

        private void OnAlarmFired(AlarmCollection alarmInfos)
        {
            this.AlarmFired?.Invoke(this, alarmInfos);
        }

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
