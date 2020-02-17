using ApplicationLayer.Models;
using ApplicationLayer.ViewModels.DialogViewModels;
using ApplicationLayer.ViewModels.DockingItemViewModels;
using ApplicationLayer.ViewModels.DocumentTypeViewModels;
using ApplicationLayer.ViewModels.Messages;
using ApplicationLayer.ViewModels.ToolWindowViewModels;
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
using System.Xml.Serialization;

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
    [XmlInclude(typeof(AlarmListViewModel))]
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
        [XmlIgnore]
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
        [XmlIgnore]
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

        public ObservableCollection<ToolWindowViewModel> ToolItems { get; } = new ObservableCollection<ToolWindowViewModel>();

        [XmlIgnore]
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

            Messenger.Default.Send<AddEditorMessage>(new AddEditorMessage(newDocument));
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

        private RelayCommand<ToolWindowViewModel> showToolWindowCommand;
        public RelayCommand<ToolWindowViewModel> ShowToolWindowCommand
        {
            get
            {
                if (this.showToolWindowCommand == null)
                    this.showToolWindowCommand = new RelayCommand<ToolWindowViewModel>(OnShowToolWindow);

                return this.showToolWindowCommand;
            }
        }
        private void OnShowToolWindow(ToolWindowViewModel obj)
        {
            if (obj == null) return;
            if (this.ToolItems.Contains(obj)) return;

            this.ToolItems.Add(obj);
        }

        private RelayCommand<List<DockingItemViewModel>> unregisterDockingWindowCommand;
        public RelayCommand<List<DockingItemViewModel>> UnRegisterDockingWindowCommand
        {
            get
            {
                if (this.unregisterDockingWindowCommand == null)
                    this.unregisterDockingWindowCommand = new RelayCommand<List<DockingItemViewModel>>(OnUnRegisterDockingWindow);

                return this.unregisterDockingWindowCommand;
            }
        }
        private void OnUnRegisterDockingWindow(List<DockingItemViewModel> obj)
        {
            if (obj == null) return;

            foreach(var item in obj)
            {
                if (item is EditorTypeViewModel) this.Documents.Remove(item as EditorTypeViewModel);
                else if (item is ToolWindowViewModel) this.ToolItems.Remove(item as ToolWindowViewModel);
            }
        }

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
            Messenger.Default.Register<AddMissedChangedFiles>(solutionExplorer, solutionExplorer.ReceivedAddMissedChangedFilesMessage);
        }

        private void InitAlarmList()
        {
            var alarmList = ServiceLocator.Current.GetInstance<AlarmListViewModel>();

            Messenger.Default.Register<AddEditorMessage>(alarmList, alarmList.ReceivedAddEditorMessage);
        }

        private void InitQuestionToSaveDialog()
        {
            var questionToSaveDialog = ServiceLocator.Current.GetInstance<QuestionToSaveViewModel>();

            Messenger.Default.Register<AddChangedFileMessage>(questionToSaveDialog, questionToSaveDialog.ReceivedAddChangedFileMessage);
            Messenger.Default.Register<RemoveChangedFileMessage>(questionToSaveDialog, questionToSaveDialog.ReceivedRemoveChangedFileMessage);
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
            this.InitAlarmList();
            this.InitQuestionToSaveDialog();

            Messenger.Default.Register<DisplayMessage>(MessageBoxLogic.Own, MessageBoxLogic.Own.ShowMessage);
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

            Messenger.Default.Send<AddEditorMessage>(new AddEditorMessage(editor));
        }
    }
}