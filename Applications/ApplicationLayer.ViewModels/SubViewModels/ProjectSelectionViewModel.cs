using ApplicationLayer.Common.Utilities;
using ApplicationLayer.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Parse.Algorithms;
using Parse.BackEnd.Target;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ApplicationLayer.ViewModels.SubViewModels
{
    public class ProjectSelectionViewModel : ViewModelBase
    {
        private Type backupData;
        private List<DetailType> terminalList = new List<DetailType>();
        private ISimilarityComparison similarity = new LikeVSSimilarityComparison();

        public ObservableCollection<ProjectData> TotalProjectList { get; private set; } = new ObservableCollection<ProjectData>();
        public ObservableCollection<ProjectData> AvailableProjectList { get; private set; } = new ObservableCollection<ProjectData>();
        public ObservableCollection<string> SupportLanguages { get; private set; } = new ObservableCollection<string>();
        public ObservableCollection<ClassHierarchyData> TotalCPUs { get; private set; } = new ObservableCollection<ClassHierarchyData>();
        public ObservableCollection<DetailType> FilterCPUs { get; private set; } = new ObservableCollection<DetailType>();

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
                this.RaisePropertyChanged(nameof(FilterMode));
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
                this.RaisePropertyChanged(nameof(FilterSelectedIndex));

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
                if (this.cpuSearch?.Length > 0)
                {
                    double topSimility = 0;

                    for (int i = 0; i < this.terminalList.Count; i++)
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
                    foreach (var item in this.TotalProjectList)
                    {
                        this.AvailableProjectList.Add(item);
                    }
                }

                this.RaisePropertyChanged(nameof(SelectedTerminalItem));
            }
        }

        private Type selectedItem;
        public Type SelectedItem
        {
            get => selectedItem;
            set
            {
                this.selectedItem = value;
                this.RaisePropertyChanged(nameof(SelectedItem));
            }
        }

        private ProjectData selectedProject;
        public ProjectData SelectedProject
        {
            get => this.selectedProject;
            set
            {
                this.selectedProject = value;
                this.RaisePropertyChanged(nameof(SelectedProject));
            }
        }

        public ProjectSelectionViewModel()
        {
            this.TotalProjectList.Add(new ProjectData() { ImageSrc = "", Grammar = new MiniCGrammar(), ProjectType = ProjectData.ProjectTypes.Project });
            this.TotalProjectList.Add(new ProjectData() { ImageSrc = "", Grammar = new MiniCGrammar(), ProjectType = ProjectData.ProjectTypes.LibraryProject });
            this.TotalProjectList.Add(new ProjectData() { ImageSrc = "", Grammar = new AJGrammar(), ProjectType = ProjectData.ProjectTypes.Project });
            this.TotalProjectList.Add(new ProjectData() { ImageSrc = "", Grammar = new AJGrammar(), ProjectType = ProjectData.ProjectTypes.LibraryProject });

            ClassHierarchyGenerator classHierarchyGenerator = new ClassHierarchyGenerator();

            this.TotalCPUs.Add(classHierarchyGenerator.ToHierarchyData(typeof(Target)));
            foreach (var item in this.TotalCPUs)
                this.terminalList.AddRange(item.GetTerminalCollection());

            this.FilterCPUs.CollectionChanged += FilterCPUs_CollectionChanged;
        }

        private void FilterCPUs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.FilterMode = (this.FilterCPUs.Count > 0) ? true : false;
        }
    }
}
