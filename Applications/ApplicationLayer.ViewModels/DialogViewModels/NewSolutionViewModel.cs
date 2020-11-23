using ApplicationLayer.ViewModels.Interfaces;
using ApplicationLayer.ViewModels.Messages;
using ApplicationLayer.ViewModels.SubViewModels;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Parse.BackEnd.Target;
using System;
using System.Linq;

namespace ApplicationLayer.ViewModels.DialogViewModels
{
    public class NewSolutionViewModel : DialogViewModel, IPathSearchable
    {
        /********************************************************************************************
         * private field section
         ********************************************************************************************/
        private string solutionName = string.Empty;
        private string solutionPath = string.Empty;
        private RelayCommand<Action<string>> searchCommand;
        private RelayCommand<Action> _createCommand;



        /********************************************************************************************
         * property section
         ********************************************************************************************/
        public ProjectSelectionViewModel ProjectSelection { get; } = new ProjectSelectionViewModel();

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

        public string Path
        {
            get => this.solutionPath;
            set
            {
                if (this.solutionPath == value) return;

                this.solutionPath = value;
                if (this.Path.Length > 0)
                {
                    if (this.solutionPath.Last() != '\\')
                        this.solutionPath += "\\";
                }
                this.RaisePropertyChanged(nameof(Path));
                this.RaisePropertyChanged(nameof(SolutionFullPath));

                CreateCommand.RaiseCanExecuteChanged();
            }
        }

        public bool CreateSolutionFolder { get; set; }
        public string SolutionFullPath { get => this.Path + this.solutionName; }



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
        public NewSolutionViewModel()
        {
            this.ProjectSelection.PropertyChanged += ProjectSelection_PropertyChanged;
        }



        /********************************************************************************************
         * command action method section
         ********************************************************************************************/
        private void OnSearch(Action<string> action)
        {
            action?.Invoke(this.Path);
        }

        private void OnCreate(Action action)
        {
            Target target = Activator.CreateInstance(this.ProjectSelection.SelectedTerminalItem) as Target;
            Messenger.Default.Send(new CreateSolutionMessage(this.Path, 
                                                                                          this.SolutionName, 
                                                                                          this.CreateSolutionFolder, 
                                                                                          this.ProjectSelection.SelectedProject.Grammar, 
                                                                                          target));

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



        /********************************************************************************************
         * event handler section
         ********************************************************************************************/
        private void ProjectSelection_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedTerminalItem") CreateCommand.RaiseCanExecuteChanged();
            else if (e.PropertyName == "SelectedProject") CreateCommand.RaiseCanExecuteChanged();
        }
    }
}
