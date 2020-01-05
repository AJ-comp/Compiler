﻿using GalaSoft.MvvmLight.Command;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using WpfApp.Models;
using WpfApp.Models.MicroControllerModels;
using WpfApp.Utilities;

namespace WpfApp.ViewModels.DialogViewModels
{
    public class NewProjectViewModel : DialogViewModel
    {
        private Generator projectGenerator = new Generator();
        public ObservableCollection<MicroController> MicroControllers { get; } = new ObservableCollection<MicroController>();

        private MicroController selectedTerminalItem;

        private MicroController selectedItem;
        public MicroController SelectedItem
        {
            get => selectedItem;
            set
            {
                if (this.selectedItem == value) return;
                this.selectedItem = value;

                this.selectedTerminalItem = (this.selectedItem is IHierarchical<MicroController>) ? null : this.selectedItem;
                this.RaisePropertyChanged("SelectedItem");

                CreateCommand.RaiseCanExecuteChanged();
            }
        }

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
            projectGenerator.GenerateSolution(this.SolutionPath, this.SolutionName, this.CreateSolutionFolder);
            action?.Invoke();
        }

        private bool CanExecuteCreate(Action action)
        {
            if (this.selectedTerminalItem == null) return false;
            if (string.IsNullOrEmpty(this.solutionPath)) return false;
            if (string.IsNullOrEmpty(this.solutionName)) return false;

            return true;
        }
    }
}
