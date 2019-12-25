using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Grammars.PracticeGrammars;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using WpfApp.ViewModels.DocumentTypeViewModels;

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

        private DocumentViewModel selectedDocument;
        public DocumentViewModel SelectedDocument
        {
            get => this.selectedDocument;
            set
            {
                this.selectedDocument = value;
                this.RaisePropertyChanged("SelectedDocument");
            }
        }

        public ViewModelBase ActivatedDialog { get; }

        public Collection<Grammar> SupplyGrammars = new Collection<Grammar>();

        private void Document_RequestClose(object sender, EventArgs e) => this._documents.Remove(sender as DocumentViewModel);

        public AlarmListViewModel AlarmListVM { get; } = new AlarmListViewModel();

        public Action NewFileAction = null;
        public Action NewProjectAction = null;
        public Action GrammarAction = null;
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
        private void OnParsingHistory() => this.ParsingHistoryAction?.Invoke();
        private void OnParseTree() => this.ParseTreeAction?.Invoke();

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            this.SupplyGrammars.Add(new MiniCGrammar());
            this.SupplyGrammars.Add(new LRTest1Grammar());

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