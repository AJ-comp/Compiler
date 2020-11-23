using ApplicationLayer.ViewModels.Interfaces;
using ApplicationLayer.ViewModels.Messages;
using ApplicationLayer.ViewModels.SubViewModels;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Parse.BackEnd.Target;
using System;

namespace ApplicationLayer.ViewModels.DialogViewModels
{
    public class NewProjectViewModel : DialogViewModel, IPathSearchable
    {
        /********************************************************************************************
         * private field section
         ********************************************************************************************/
        private string projectPath = string.Empty;
        private string projectName = string.Empty;
        private RelayCommand<Action<string>> searchCommand;
        private RelayCommand<Action> _createCommand;



        /********************************************************************************************
         * property section
         ********************************************************************************************/
        public ProjectSelectionViewModel ProjectSelection { get; } = new ProjectSelectionViewModel();

        public string Path
        {
            get => this.projectPath;
            set
            {
                this.projectPath = value;
                this.RaisePropertyChanged(nameof(Path));

                CreateCommand.RaiseCanExecuteChanged();
            }
        }

        public string ProjectName
        {
            get => this.projectName;
            set
            {
                if (this.projectName == value) return;

                this.projectName = value;
                this.RaisePropertyChanged(nameof(ProjectName));

                CreateCommand.RaiseCanExecuteChanged();
            }
        }



        /********************************************************************************************
         * command property section
         ********************************************************************************************/
        public RelayCommand<Action<string>> SearchCommand
        {
            get
            {
                if (this.searchCommand == null) this.searchCommand = new RelayCommand<Action<string>>(this.OnSearch);

                return this.searchCommand;
            }
        }

        public RelayCommand<Action> CreateCommand
        {
            get
            {
                if (this._createCommand == null)
                    this._createCommand = new RelayCommand<Action>(this.OnCreate, this.CanExecuteCreate);

                return this._createCommand;
            }
        }



        /********************************************************************************************
         * constructor section
         ********************************************************************************************/
        public NewProjectViewModel()
        {
            this.ProjectSelection.PropertyChanged += ProjectSelection_PropertyChanged;
        }



        /********************************************************************************************
         * command action method section
         ********************************************************************************************/
        private void OnSearch(Action<string> action)
        {
            action?.Invoke(this.projectPath);
        }

        private void OnCreate(Action action)
        {
            Target target = Activator.CreateInstance(this.ProjectSelection.SelectedTerminalItem) as Target;
            Messenger.Default.Send(new AddProjectMessage(System.IO.Path.Combine(this.projectPath, this.projectName), 
                                                                                     this.projectName, 
                                                                                     this.ProjectSelection.SelectedProject.Grammar, 
                                                                                     target));

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



        /********************************************************************************************
         * event handler section
         ********************************************************************************************/
        private void ProjectSelection_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "SelectedTerminalItem") CreateCommand.RaiseCanExecuteChanged();
            else if(e.PropertyName == "SelectedProject") CreateCommand.RaiseCanExecuteChanged();
        }
    }
}
