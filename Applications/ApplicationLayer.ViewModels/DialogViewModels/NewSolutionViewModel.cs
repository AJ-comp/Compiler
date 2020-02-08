using ApplicationLayer.ViewModels.Messages;
using ApplicationLayer.ViewModels.SubViewModels;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Parse.BackEnd.Target;
using System;
using System.Linq;

namespace ApplicationLayer.ViewModels.DialogViewModels
{
    public class NewSolutionViewModel : DialogViewModel
    {
        public ProjectSelectionViewModel ProjectSelection { get; } = new ProjectSelectionViewModel();

        private string solutionName = string.Empty;
        public string SolutionName
        {
            get => this.solutionName;
            set
            {
                if (this.solutionName == value) return;

                this.solutionName = value;
                this.RaisePropertyChanged(nameof(SolutionName));
                this.RaisePropertyChanged(nameof(SolutionFullPath));

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
                if (this.SolutionPath.Length > 0)
                {
                    if (this.solutionPath.Last() != '\\')
                        this.solutionPath += "\\";
                }
                this.RaisePropertyChanged(nameof(SolutionPath));
                this.RaisePropertyChanged(nameof(SolutionFullPath));

                CreateCommand.RaiseCanExecuteChanged();
            }
        }

        public bool CreateSolutionFolder { get; set; }
        public string SolutionFullPath { get => this.SolutionPath + this.solutionName; }

        private RelayCommand<Action> searchCommand;
        public RelayCommand<Action> SearchCommand
        {
            get
            {
                if (this.searchCommand == null) this.searchCommand = new RelayCommand<Action>(this.OnSearch);

                return this.searchCommand;
            }
        }
        private void OnSearch(Action action)
        {
            action?.Invoke();
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
            Target target = Activator.CreateInstance(this.ProjectSelection.SelectedTerminalItem) as Target;
            Messenger.Default.Send(new CreateSolutionMessage(this.SolutionPath, this.SolutionName, this.CreateSolutionFolder, this.ProjectSelection.SelectedProject.Grammar, target));

            action?.Invoke();
        }

        private bool CanExecuteCreate(Action action)
        {
            if (this.ProjectSelection.SelectedTerminalItem == null) return false;
            if (this.ProjectSelection.SelectedProject == null) return false;
            if (string.IsNullOrEmpty(this.solutionPath)) return false;
            if (string.IsNullOrEmpty(this.solutionName)) return false;

            return true;
        }

        public NewSolutionViewModel()
        {
            this.ProjectSelection.PropertyChanged += ProjectSelection_PropertyChanged;
        }

        private void ProjectSelection_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedTerminalItem") CreateCommand.RaiseCanExecuteChanged();
            else if (e.PropertyName == "SelectedProject") CreateCommand.RaiseCanExecuteChanged();
        }
    }
}
