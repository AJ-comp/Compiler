using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Grammars.PracticeGrammars;
using Parse.FrontEnd.Parsers.LR;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using WpfApp.ViewModels.DialogViewModels;
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
        private Collection<Grammar> supplyGrammars = new Collection<Grammar>();

        #region Property related to Document
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
                {
                    document.CloseRequest += Document_RequestClose;
                    document.AllCloseExceptThisRequest += Document_AllCloseExceptThisRequest;
                }
            if (e.OldItems != null && e.OldItems.Count != 0)
                foreach (DocumentViewModel document in e.OldItems)
                {
                    document.CloseRequest -= Document_RequestClose;
                    document.AllCloseExceptThisRequest -= Document_AllCloseExceptThisRequest;
                }
        }

        private void Document_AllCloseExceptThisRequest(object sender, EventArgs e)
        {
            this._documents.Clear();
            this._documents.Add(sender as DocumentViewModel);
        }

        private void Document_RequestClose(object sender, EventArgs e)
        {
            this._documents.Remove(sender as DocumentViewModel);
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
        #endregion

        private ObservableCollection<DialogViewModel> window;
        public ObservableCollection<DialogViewModel> Window
        {
            get
            {
                if (this.window == null)
                {
                    this.window = new ObservableCollection<DialogViewModel>();
                    this.window.CollectionChanged += _documents_CollectionChanged;
                }

                return this.window;
            }
        }

        public ViewModelBase ActivatedDialog { get; }
        public AlarmListViewModel AlarmListVM { get; } = new AlarmListViewModel();

        public Action NewFileAction = null;
        public Action NewProjectAction = null;
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
        private void OnGrammar()
        {
            var document = ServiceLocator.Current.GetInstance<GrammarInfoViewModel>();

            if(this._documents.Contains(document) == false) this._documents.Add(document);
            this.SelectedDocument = document;
        }
        private void OnParsingHistory()
        {
            var document = ServiceLocator.Current.GetInstance<ParsingHistoryViewModel>();
//            document.ParsingHistory = 

            if (this._documents.Contains(document) == false) this._documents.Add(document);
            this.selectedDocument = document;
        }
        private void OnParseTree() => this.ParseTreeAction?.Invoke();

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            this.supplyGrammars.Add(new MiniCGrammar());
            this.supplyGrammars.Add(new LRTest1Grammar());

            var grmmarViewModel = ServiceLocator.Current.GetInstance<GrammarInfoViewModel>();

            foreach(var grammar in this.supplyGrammars)
                grmmarViewModel.Grammars.Add(grammar);


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