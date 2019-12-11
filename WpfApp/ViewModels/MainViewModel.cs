using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace WpfApp.ViewModels
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private ObservableCollection<DocumentViewModel> _documents;
        public ObservableCollection<DocumentViewModel> Documents
        {
            get
            {
                if (this._documents == null)
                {
                    this._documents = new ObservableCollection<DocumentViewModel>();
                    this._documents.CollectionChanged += _documents_CollectionChanged;
                }

                return this._documents;
            }
        }

        private void _documents_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count != 0)
                foreach (DocumentViewModel document in e.NewItems)
                    document.RequestClose += Document_RequestClose;
            if (e.OldItems != null && e.OldItems.Count != 0)
                foreach (DocumentViewModel document in e.NewItems)
                    document.RequestClose -= Document_RequestClose;
        }

        private void Document_RequestClose(object sender, EventArgs e) => this._documents.Remove(sender as DocumentViewModel);

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

        private void OnNewFile() => this.NewFileAction?.Invoke();
        private void OnGrammar() => this.GrammarAction?.Invoke();
        private void OnCanonicalTable() => this.CanonicalTableAction?.Invoke();
        private void OnParsingTable() => this.ParsingTableAction?.Invoke();
        private void OnParsingHistory() => this.ParsingHistoryAction?.Invoke();
        private void OnParseTree() => this.ParseTreeAction?.Invoke();

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
            }
            else
            {
                // Code runs "for real"
            }
        }
    }
}