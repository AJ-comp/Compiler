using ApplicationLayer.Common.Utilities;
using ApplicationLayer.ViewModels.DialogViewModels;
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
using Parse.FrontEnd.Parsers.ErrorHandling.GrammarPrivate.MiniC_LR;
using Parse.FrontEnd.Parsers.LR;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

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
                this.RaisePropertyChanged(nameof(IsDebugStatus));
            }
        }

        public SolutionExplorerViewModel SolutionExplorer { get; } = ServiceLocator.Current.GetInstance<SolutionExplorerViewModel>();

        private List<ToolWindowViewModel> allToolItems = new List<ToolWindowViewModel>();
        public ReadOnlyCollection<ToolWindowViewModel> AllToolItems => new ReadOnlyCollection<ToolWindowViewModel>(allToolItems);

        public ObservableCollection<ToolWindowViewModel> VisibleToolItems { get; } = new ObservableCollection<ToolWindowViewModel>();

        public Action ParseTreeAction = null;

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

            if (this.SolutionExplorer.Documents.Contains(document) == false) this.SolutionExplorer.Documents.Add(document);
            this.SolutionExplorer.SelectedDocument = document;
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
            if (this.VisibleToolItems.Contains(obj)) return;

            this.VisibleToolItems.Add(obj);
        }

        private void InitParserToUse()
        {
            foreach(var grammar in this.supplyGrammars)
            {
                ParserFactory.Instance.RegisterParser(ParserFactory.ParserKind.SLR_Parser, grammar);

                if (grammar.ToString() == new MiniCGrammar().ToString())
                {
                    LRParser parser = ParserFactory.Instance.GetParser(ParserFactory.ParserKind.SLR_Parser, grammar) as LRParser;
                    MiniC_LRErrorHandlerFactory.Instance.AddErrorHandler(parser);
                }
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
            Messenger.Default.Register<AddMissedChangedFilesMessage>(solutionExplorer, solutionExplorer.ReceivedAddMissedChangedFilesMessage);
        }

        private void InitAlarmList()
        {
            var alarmList = ServiceLocator.Current.GetInstance<AlarmListViewModel>();

            Messenger.Default.Register<AlarmMessage>(alarmList, alarmList.ReceivedAlarmMessage);
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
            this.allToolItems.Add(ServiceLocator.Current.GetInstance<SolutionExplorerViewModel>());
            this.allToolItems.Add(ServiceLocator.Current.GetInstance<AlarmListViewModel>());

            this.supplyGrammars.Add(new MiniCGrammar());
            this.supplyGrammars.Add(new LRTest1Grammar());

            this.InitParserToUse();
            this.InitGrammarWindow();
            this.InitSolutionExplorer();
            this.InitAlarmList();
            this.InitQuestionToSaveDialog();

            Messenger.Default.Register<DisplayMessage>(MessageBoxLogic.Instance, MessageBoxLogic.Instance.ShowMessage);

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