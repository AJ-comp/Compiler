using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;

namespace WpfApp.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        public AlarmListViewModel AlarmListVM { get; } = new AlarmListViewModel();

        public Action NewFileAction = null;
        public Action NewProjectAction = null;
        public Action GrammarAction = null;
        public Action CanonicalTableAction = null;
        public Action ParsingTableAction = null;
        public Action ParsingHistoryAction = null;
        public Action ParseTreeAction = null;

        private RelayCommand _newFileCommand;
        public RelayCommand NewFileCommand
        {
            get
            {
                if (_newFileCommand == null)
                    _newFileCommand = new RelayCommand(this.OnNewFile);

                return _newFileCommand;
            }
        }

        private RelayCommand _grammarCommand;
        public RelayCommand GrammarCommand
        {
            get
            {
                if (_grammarCommand == null)
                    _grammarCommand = new RelayCommand(this.OnGrammar);

                return _grammarCommand;
            }
        }

        private RelayCommand _canonicalTableCommand;
        public RelayCommand CanonicalTableCommand
        {
            get
            {
                if (_canonicalTableCommand == null)
                    _canonicalTableCommand = new RelayCommand(this.OnCanonicalTable);

                return _canonicalTableCommand;
            }
        }

        private RelayCommand _parsingTableCommand;
        public RelayCommand ParsingTableCommand
        {
            get
            {
                if (_parsingTableCommand == null)
                    _parsingTableCommand = new RelayCommand(this.OnParsingTable);

                return _parsingTableCommand;
            }
        }

        private RelayCommand _parsingHistoryCommand;
        public RelayCommand ParsingHistoryCommand
        {
            get
            {
                if (_parsingHistoryCommand == null)
                    _parsingHistoryCommand = new RelayCommand(this.OnParsingHistory);

                return _parsingHistoryCommand;
            }
        }

        private RelayCommand _parseTreeCommand;
        public RelayCommand ParseTreeCommand
        {
            get
            {
                if (_parseTreeCommand == null)
                    _parseTreeCommand = new RelayCommand(this.OnParseTree);

                return _parseTreeCommand;
            }
        }

        public MainWindowViewModel()
        {
        }

        private void OnNewFile() => this.NewFileAction?.Invoke();
        private void OnGrammar() => this.GrammarAction?.Invoke();
        private void OnCanonicalTable() => this.CanonicalTableAction?.Invoke();
        private void OnParsingTable() => this.ParsingTableAction?.Invoke();
        private void OnParsingHistory() => this.ParsingHistoryAction?.Invoke();
        private void OnParseTree() => this.ParseTreeAction?.Invoke();
    }
}
