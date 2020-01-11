using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.WindowsAPICodePack.Dialogs;
using Parse.BackEnd.Target;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.WpfControls.Algorithms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using WpfApp.Messages;
using WpfApp.Models;
using WpfApp.Utilities;
using WpfApp.Utilities.GeneratorPackages.ProjectStructs;

namespace WpfApp.ViewModels.DialogViewModels
{
    public class NewProjectViewModel : DialogViewModel
    {
        private Type backupData;
        private SolutionGenerator solutionGenerator = new SolutionGenerator();
        private List<DetailType> terminalList = new List<DetailType>();
        private ISimilarityComparison similarity = new LikeVSSimilarityComparison();

        public ObservableCollection<ProjectData> TotalProjectList { get; private set; } = new ObservableCollection<ProjectData>();
        public ObservableCollection<ProjectData> AvailableProjectList { get; private set; } = new ObservableCollection<ProjectData>();
        public ObservableCollection<string> SupportLanguages { get; private set; } = new ObservableCollection<string>();
        public ObservableCollection<ClassHierarchyData> TotalCPUs { get; private set; } = new ObservableCollection<ClassHierarchyData>();
        public ObservableCollection<DetailType> FilterCPUs { get; private set; } = new ObservableCollection<DetailType>();

        private bool filterMode;
        public bool FilterMode
        {
            get => this.filterMode;
            set
            {
                if (this.filterMode == value) return;
                this.filterMode = value;
                if (this.filterMode) backupData = this.selectedTerminalItem;
                else this.SelectedTerminalItem = backupData;
                this.RaisePropertyChanged("FilterMode");
            }
        }

        private int filterSelectedIndex = -1;
        public int FilterSelectedIndex
        {
            get => this.filterSelectedIndex;
            set
            {
                if (this.filterSelectedIndex == value) return;
                this.filterSelectedIndex = value;
                this.RaisePropertyChanged("FilterSelectedIndex");

                if (this.filterSelectedIndex >= 0)
                    this.SelectedTerminalItem = this.terminalList[this.filterSelectedIndex].Type;
            }
        }

        private string cpuSearch;
        public string CPUSearch
        {
            get => this.cpuSearch;
            set
            {
                if (this.cpuSearch == value) return;
                this.cpuSearch = value;
                this.FilterCPUs.Clear();

                int topSimilityIndex = -1;
                if(this.cpuSearch.Length > 0)
                {
                    double topSimility = 0;

                    for(int i=0; i<this.terminalList.Count; i++)
                    {
                        var t = this.terminalList[i];

                        List<uint> matchedIndexes = new List<uint>();
                        var candidateSimility = this.similarity.SimilarityValue(t.Type.Name, this.CPUSearch, out matchedIndexes);
                        t.MatchedIndexes = matchedIndexes;

                        if (matchedIndexes.Count > 0)
                        {
                            this.FilterCPUs.Add(t);
                        }

                        if (candidateSimility > topSimility)
                        {
                            topSimility = candidateSimility;
                            topSimilityIndex = i;
                        }
                    }
                }

                if (topSimilityIndex >= 0) this.FilterSelectedIndex = topSimilityIndex;
            }
        }

        private Type selectedTerminalItem;
        public Type SelectedTerminalItem
        {
            get => this.selectedTerminalItem;
            private set
            {
                if (this.selectedTerminalItem == value) return;

                this.selectedTerminalItem = value;
                this.AvailableProjectList.Clear();

                // if selected terminal item then show project list
                if (this.selectedTerminalItem != null)
                {
                    foreach(var item in this.TotalProjectList)
                    {
                        this.AvailableProjectList.Add(item);
                    }
                }

                this.RaisePropertyChanged("SelectedTerminalItem");
                CreateCommand.RaiseCanExecuteChanged();
            }
        }

        private Type selectedItem;
        public Type SelectedItem
        {
            get => selectedItem;
            set
            {
                this.selectedItem = value;
                this.RaisePropertyChanged("SelectedItem");
            }
        }

        private ProjectData selectedProject;
        public ProjectData SelectedProject
        {
            get => this.selectedProject;
            set
            {
                this.selectedProject = value;
                this.RaisePropertyChanged("SelectedProject");
                CreateCommand.RaiseCanExecuteChanged();
            }
        }

        #region Functionality related to Total Display
        private string solutionName = string.Empty;
        public string SolutionName
        {
            get => this.solutionName;
            set
            {
                if (this.solutionName == value) return;

                this.solutionName = value;
                this.RaisePropertyChanged("SolutionName");
                this.RaisePropertyChanged("SolutionFullPath");

                CreateCommand.RaiseCanExecuteChanged();
            }
        }

        private string solutionPath = string.Empty;
        public string SolutionPath
        {
            get => this.solutionPath;
            set
            {
                if (this.solutionPath == value) return;

                this.solutionPath = value;
                if(this.SolutionPath.Length > 0)
                {
                    if (this.solutionPath.Last() != '\\')
                        this.solutionPath += "\\";
                }
                this.RaisePropertyChanged("SolutionPath");
                this.RaisePropertyChanged("SolutionFullPath");

                CreateCommand.RaiseCanExecuteChanged();
            }
        }

        public bool CreateSolutionFolder { get; set; }
        public string SolutionFullPath { get => this.SolutionPath + this.solutionName; }

        private RelayCommand searchCommand;
        public RelayCommand SearchCommand
        {
            get
            {
                if (this.searchCommand == null) this.searchCommand = new RelayCommand(this.OnSearch);

                return this.searchCommand;
            }
        }
        private void OnSearch()
        {
            CommonOpenFileDialog selectFolderDialog = new CommonOpenFileDialog();

            selectFolderDialog.InitialDirectory = "C:\\Users";
            selectFolderDialog.IsFolderPicker = true;
            if (selectFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                this.SolutionPath = selectFolderDialog.FileName + "\\";
            }
        }

        private RelayCommand<Action> _createCommand;
        public RelayCommand<Action> CreateCommand
        {
            get
            {
                if (this._createCommand == null)
                    this._createCommand = new RelayCommand<Action>(this.OnCreate, this.CanExecuteCreate);

                return this._createCommand;
            }
        }
        private void OnCreate(Action action)
        {
            Target target = Activator.CreateInstance(this.SelectedTerminalItem) as Target;
            Messenger.Default.Send(new CreateSolutionMessage(this.SolutionPath, this.SolutionName, this.CreateSolutionFolder, this.SelectedProject.Grammar, target));

            action?.Invoke();
        }

        private bool CanExecuteCreate(Action action)
        {
            if (this.SelectedTerminalItem == null) return false;
            if (this.SelectedProject == null) return false;
            if (string.IsNullOrEmpty(this.solutionPath)) return false;
            if (string.IsNullOrEmpty(this.solutionName)) return false;

            return true;
        }

        private RelayCommand<ClassHierarchyData> cpuSelectedCommand;
        public RelayCommand<ClassHierarchyData> CPUSelectedCommand
        {
            get
            {
                if (this.cpuSelectedCommand == null)
                    this.cpuSelectedCommand = new RelayCommand<ClassHierarchyData>(OnCPUSelected);

                return this.cpuSelectedCommand;
            }
        }

        private void OnCPUSelected(ClassHierarchyData selected)
        {
            this.SelectedItem = selected.Data;
            this.SelectedTerminalItem = (selected.Items.Count == 0) ? selected.Data : null;
        }


        private RelayCommand<string> navigateCommand;
        public RelayCommand<string> NavigateCommand
        {
            get
            {
                if (this.navigateCommand == null)
                    this.navigateCommand = new RelayCommand<string>((uri) =>
                    {
                        Process.Start(new ProcessStartInfo(uri));
                    });

                return navigateCommand;
            }
        }
        #endregion

        public NewProjectViewModel()
        {
            this.TotalProjectList.Add(new ProjectData() { ImageSrc = "", Grammar = new MiniCGrammar(), ProjectType = ProjectData.ProjectTypes.Project });
            this.TotalProjectList.Add(new ProjectData() { ImageSrc = "", Grammar = new MiniCGrammar(), ProjectType = ProjectData.ProjectTypes.LibraryProject });
            this.TotalProjectList.Add(new ProjectData() { ImageSrc = "", Grammar = new AJGrammar(), ProjectType = ProjectData.ProjectTypes.Project });
            this.TotalProjectList.Add(new ProjectData() { ImageSrc = "", Grammar = new AJGrammar(), ProjectType = ProjectData.ProjectTypes.LibraryProject });

            ClassHierarchyGenerator classHierarchyGenerator = new ClassHierarchyGenerator();

            this.TotalCPUs.Add(classHierarchyGenerator.ToHierarchyData(typeof(Target)));
            foreach(var item in this.TotalCPUs)
                this.terminalList.AddRange(item.GetTerminalCollection());

            this.FilterCPUs.CollectionChanged += FilterCPUs_CollectionChanged;
        }

        private void FilterCPUs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.FilterMode = (this.FilterCPUs.Count > 0) ? true : false;
        }
    }
}
