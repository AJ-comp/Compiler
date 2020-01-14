using ApplicationLayer.ViewModels;
using ApplicationLayer.ViewModels.DialogViewModels;
using ApplicationLayer.ViewModels.DocumentTypeViewModels;
using ApplicationLayer.ViewModels.Messages;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Grammars.PracticeGrammars;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using WpfApp.ViewModels.WindowViewModels;

namespace ApplicationLayer.WpfApp.ViewModels
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

        #region Command related to NewFile
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
        private void OnNewFile() => this.NewFileAction?.Invoke();
        #endregion

        #region Command related to NewProject
        private RelayCommand newProjectCommand;
        public RelayCommand NewProjectCommand
        {
            get
            {
                if (newProjectCommand == null)
                    newProjectCommand = new RelayCommand(this.OnNewProject);

                return newProjectCommand;
            }
        }
        private void OnNewProject() => this.NewProjectAction?.Invoke();
        #endregion

        #region Command related to Grammar
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
        private void OnGrammar()
        {
            var document = ServiceLocator.Current.GetInstance<GrammarInfoViewModel>();

            if (this._documents.Contains(document) == false) this._documents.Add(document);
            this.SelectedDocument = document;
        }
        #endregion

        #region Command related to ParsingHistory
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
        private void OnParsingHistory()
        {
            var document = ServiceLocator.Current.GetInstance<ParsingHistoryViewModel>();
            //            document.ParsingHistory = 

            if (this._documents.Contains(document) == false) this._documents.Add(document);
            this.selectedDocument = document;
        }
        #endregion

        #region Command related to ParseTree
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
        private void OnParseTree() => this.ParseTreeAction?.Invoke();
        #endregion

        #region Command related to Option
        private RelayCommand optionCommand;
        public RelayCommand OptionCommand
        {
            get
            {
                if (optionCommand == null)
                    optionCommand = new RelayCommand(this.OnOption);

                return optionCommand;
            }
        }
        private void OnOption()
        {
            // for test
            var app = (App)Application.Current;

            List<Uri> uris = new List<Uri>
            {
                new Uri("Resources/BasicImageResources.xaml", UriKind.RelativeOrAbsolute),
                new Uri("/Wpf.UI.Basic;component/BlueTheme.xaml", UriKind.RelativeOrAbsolute),
                new Uri("/Wpf.UI.Advance;component/BlueTheme.xaml", UriKind.RelativeOrAbsolute)
            };

            app.ChangeTheme(uris);
        }
        #endregion


        private void InitGrammarWindow()
        {
            var grmmarViewModel = ServiceLocator.Current.GetInstance<GrammarInfoViewModel>();
            foreach (var grammar in this.supplyGrammars)
                grmmarViewModel.Grammars.Add(grammar);
        }

        private void InitNewProjectWindow()
        {
            var newProject = ServiceLocator.Current.GetInstance<NewProjectViewModel>();
        }

        private void InitSolutionExplorer()
        {
            var solutionExplorer = ServiceLocator.Current.GetInstance<SolutionExplorerViewModel>();
            Messenger.Default.Register<CreateSolutionMessage>(this, solutionExplorer.ReceivedCreateSolutionMessage);
            Messenger.Default.Register<LoadSolutionMessage>(this, solutionExplorer.ReceivedLoadSolutionMessage);
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            this.supplyGrammars.Add(new MiniCGrammar());
            this.supplyGrammars.Add(new LRTest1Grammar());

            this.InitGrammarWindow();
            this.InitNewProjectWindow();
            this.InitSolutionExplorer();

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