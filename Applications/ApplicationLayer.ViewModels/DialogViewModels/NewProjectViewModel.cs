using ApplicationLayer.ViewModels.Messages;
using ApplicationLayer.ViewModels.SubViewModels;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Parse.BackEnd.Target;
using System;
using System.IO;

namespace ApplicationLayer.ViewModels.DialogViewModels
{
    public class NewProjectViewModel : DialogViewModel
    {
        public ProjectSelectionViewModel ProjectSelection { get; } = new ProjectSelectionViewModel();

        private string projectPath = string.Empty;
        public string ProjectPath
        {
            get => this.projectPath;
            set
            {
                this.projectPath = value;
                this.RaisePropertyChanged("ProjectPath");

                CreateCommand.RaiseCanExecuteChanged();
            }
        }

        private string projectName = string.Empty;
        public string ProjectName
        {
            get => this.projectName;
            set
            {
                if (this.projectName == value) return;

                this.projectName = value;
                this.RaisePropertyChanged("ProjectName");

                CreateCommand.RaiseCanExecuteChanged();
            }
        }

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
            Messenger.Default.Send(new AddProjectMessage(Path.Combine(this.projectPath, this.projectName), this.projectName, this.ProjectSelection.SelectedProject.Grammar, target));

            action?.Invoke();
        }

        private bool CanExecuteCreate(Action action)
        {
            if (this.ProjectSelection.SelectedTerminalItem == null) return false;
            if (this.ProjectSelection.SelectedProject == null) return false;
            if (string.IsNullOrEmpty(this.projectPath)) return false;
            if (string.IsNullOrEmpty(this.projectName)) return false;

            return true;
        }

        public NewProjectViewModel()
        {
            this.ProjectSelection.PropertyChanged += ProjectSelection_PropertyChanged;
        }

        private void ProjectSelection_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "SelectedTerminalItem") CreateCommand.RaiseCanExecuteChanged();
            else if(e.PropertyName == "SelectedProject") CreateCommand.RaiseCanExecuteChanged();
        }
    }
}
