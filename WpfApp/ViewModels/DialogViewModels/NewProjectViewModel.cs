using GalaSoft.MvvmLight.Command;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Linq;
using WpfApp.Utilities;

namespace WpfApp.ViewModels.DialogViewModels
{
    public class NewProjectViewModel : DialogViewModel
    {
        private Generator projectGenerator = new Generator();

        private string solutionName = string.Empty;
        public string SolutionName
        {
            get => this.solutionName;
            set
            {
                this.solutionName = value;
                this.RaisePropertyChanged("SolutionName");
                this.RaisePropertyChanged("SolutionFullPath");
            }
        }

        private string solutionPath = string.Empty;
        public string SolutionPath
        {
            get => this.solutionPath;
            set
            {
                this.solutionPath = value;
                if(this.SolutionPath.Length > 0)
                {
                    if (this.solutionPath.Last() != '\\')
                        this.solutionPath += "\\";
                }
                this.RaisePropertyChanged("SolutionPath");
                this.RaisePropertyChanged("SolutionFullPath");
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

        protected override void OnCreate(Action action)
        {
            projectGenerator.GenerateSolution(this.SolutionPath, this.SolutionName, this.CreateSolutionFolder);


            base.OnCreate(action);
        }
    }
}
