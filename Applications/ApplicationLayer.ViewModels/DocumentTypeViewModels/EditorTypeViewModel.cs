﻿using ApplicationLayer.Models.Invokers;
using GalaSoft.MvvmLight.Command;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
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
        public Grammar Grammar { get; } = new MiniCGrammar();

        public int caretIndex = 0;
        public int CaretIndex
        {
            get => caretIndex;
            set
            {
                this.caretIndex = value;
                this.RaisePropertyChanged("CaretIndex");
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