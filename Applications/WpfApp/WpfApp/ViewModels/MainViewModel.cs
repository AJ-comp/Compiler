using ApplicationLayer.Models;
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
using System.IO;
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

        private bool isDebugStatus;
        public bool IsDebugStatus
        {
            get => this.isDebugStatus;
            set
            {
                this.isDebugStatus = value;
                this.RaisePropertyChanged("IsDebugStatus");
            }
        }

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

        public Action ParseTreeAction = null;

        #region Command related to NewFile
        private RelayCommand<Func<Document>> _newFileCommand;
        public RelayCommand<Func<Document>> NewFileCommand
        {
            get
            {
                if (_newFileCommand == null)
                    _newFileCommand = new RelayCommand<Func<Document>>(this.OnNewFile);

                return _newFileCommand;
            }
        }
        private void OnNewFile(Func<Document> func)
        {
            if (func == null) return;

            var document = func.Invoke();
            if (document == null) return;

            var newDocument = new EditorTypeViewModel();
            this.Documents.Add(newDocument);
            this.SelectedDocument = newDocument;

            this.AlarmListVM.AddEditors(newDocument);
        }
        #endregion

        #region Command related to Open
        private RelayCommand<Func<string>> openCommand;
        public RelayCommand<Func<string>> OpenCommand
        {
            get
            {
                if (openCommand == null)
                    openCommand = new RelayCommand<Func<string>>(this.OnOpenProject);

                return openCommand;
            }
        }
        private void OnOpenProject(Func<string> func)
        {
            string selSolutionFullPath = func?.Invoke();
            if (string.IsNullOrEmpty(selSolutionFullPath)) return;

            Messenger.Default.Send(new LoadSolutionMessage(selSolutionFullPath));
        }
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

        private void InitSolutionExplorer()
        {
            var solutionExplorer = ServiceLocator.Current.GetInstance<SolutionExplorerViewModel>();

            Messenger.Default.Register<CreateSolutionMessage>(solutionExplorer, solutionExplorer.ReceivedCreateSolutionMessage);
            Messenger.Default.Register<LoadSolutionMessage>(solutionExplorer, solutionExplorer.ReceivedLoadSolutionMessage);
            Messenger.Default.Register<AddProjectMessage>(solutionExplorer, solutionExplorer.ReceivedAddNewProjectMessage);
        }

        private void InitQuestionToSaveDialog()
        {
            var questionToSaveDialog = ServiceLocator.Current.GetInstance<QuestionToSaveViewModel>();

            Messenger.Default.Register<ChangedFileMessage>(questionToSaveDialog, questionToSaveDialog.ReceivedUpdateChangedFileMessage);
            Messenger.Default.Register<GetChangedListMessage>(questionToSaveDialog, questionToSaveDialog.ReceivedGetChangedFileListMessage);
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            this.supplyGrammars.Add(new MiniCGrammar());
            this.supplyGrammars.Add(new LRTest1Grammar());

            this.InitGrammarWindow();
            this.InitSolutionExplorer();
            this.InitQuestionToSaveDialog();

            Messenger.Default.Register<OpenFileMessage>(this, this.ReceivedOpenFileMessage);

            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
            }
            else
            {
                // Code runs "for real"
            }
        }


        public void ReceivedOpenFileMessage(OpenFileMessage message)
        {
            string fileName = message.SelectedFile.FullPath;

            if (File.Exists(fileName) == false) return;
            string content = File.ReadAllText(fileName);

            var editor = new EditorTypeViewModel(fileName, content);
            if (this.Documents.Contains(editor)) return;

            this.Documents.Add(editor);
            this.SelectedDocument = editor;

            this.AlarmListVM.AddEditors(editor);
        }


        public void ReceivedChangedFileListMessage(ChangedFileMessage message)
        {

        }
    }
}