using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Parse.WpfControls.Algorithms;
using Parse.WpfControls.SyntaxEditorComponents.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Parse.WpfControls.SyntaxEditorComponents.ViewModels
{
    public class CompletionListViewModel : ViewModelBase
    {
        /// <summary>
        /// This member means filtered candidate by the input string.
        /// </summary>
        private CompletionItem[] filteredCandidate;
        private ISimilarityComparison similarity = new VSLikeSimilarityComparison();

        public string PropertyImgSrc { get; } = "pack://application:,,,/Parse.WpfControls;component/Resources/monkey_spanner.png";
        public string KeywordImgSrc { get; } = "pack://application:,,,/Parse.WpfControls;component/Resources/monkey_spanner.png";
        public string EnumImgSrc { get; } = "pack://application:,,,/Parse.WpfControls;component/Resources/monkey_spanner.png";
        public string CodeSnippImgSrc { get; } = "pack://application:,,,/Parse.WpfControls;component/Resources/monkey_spanner.png";
        public string NamespaceImgSrc { get; } = "pack://application:,,,/Parse.WpfControls;component/Resources/monkey_spanner.png";
        public string FunctionImgSrc { get; } = "pack://application:,,,/Parse.WpfControls;component/Resources/monkey_spanner.png";

        public SortedSet<CompletionItem> TotalCollection { get; } = new SortedSet<CompletionItem>(new CompletionItemComparer());
        public HashSet<CompletionItem> AvailableCollection { get; private set; } = new HashSet<CompletionItem>();
        public ObservableCollection<CompletionItem> CandidateCollection { get; private set; } = new ObservableCollection<CompletionItem>();

        private bool isFilteringProperty;
        public bool IsFilteringProperty
        {
            get => this.isFilteringProperty;
            set
            {
                this.isFilteringProperty = value;
                this.RaisePropertyChanged("IsFilteringProperty");
                this.OnFilteringChanged();
            }
        }

        private bool isFilteringKeyword;
        public bool IsFilteringKeyword
        {
            get { return isFilteringKeyword; }
            set
            {
                isFilteringKeyword = value;
                this.RaisePropertyChanged("IsFilteringKeyword");
                this.OnFilteringChanged();
            }
        }

        private bool isFilteringEnum;
        public bool IsFilteringEnum
        {
            get { return isFilteringEnum; }
            set
            {
                isFilteringEnum = value;
                this.RaisePropertyChanged("IsFilteringEnum");
                this.OnFilteringChanged();
            }
        }

        private bool isFilteringCodeSnipp;
        public bool IsFilteringCodeSnipp
        {
            get { return isFilteringCodeSnipp; }
            set
            {
                isFilteringCodeSnipp = value;
                this.RaisePropertyChanged("IsFilteringCodeSnipp");
                this.OnFilteringChanged();
            }
        }

        private bool isFilteringNamespace;
        public bool IsFilteringNamespace
        {
            get { return isFilteringNamespace; }
            set
            {
                isFilteringNamespace = value;
                this.RaisePropertyChanged("IsFilteringNamespace");
                this.OnFilteringChanged();
            }
        }

        private bool isFilteringFunction;
        public bool IsFilteringFunction
        {
            get { return isFilteringFunction; }
            set
            {
                isFilteringFunction = value;
                this.RaisePropertyChanged("IsFilteringFunction");
                this.OnFilteringChanged();
            }
        }

        private int selectedIndex = 0;
        public int SelectedIndex
        {
            get => this.selectedIndex;
            set
            {
                this.selectedIndex = value;
                this.RaisePropertyChanged("SelectedIndex");
            }

        }

        private string inputString = string.Empty;
        public string InputString
        {
            get => this.inputString;
            set
            {
                this.inputString = value;
                this.RaisePropertyChanged("InputString");
                this.OnInputStringChanged();
            }
        }

        public RelayCommand FilterButtonClickCmd { get; }

        public event EventHandler RequestFilterButtonClick;

        public CompletionListViewModel()
        {
            this.FilterButtonClickCmd = new RelayCommand(() => this.RequestFilterButtonClick?.Invoke(this, null));
        }

        private bool IsFilteringAllDisable()
        {
            if (this.isFilteringProperty) return false;
            if (this.isFilteringKeyword) return false;
            if (this.isFilteringEnum) return false;
            if (this.isFilteringCodeSnipp) return false;
            if (this.isFilteringFunction) return false;

            return true;
        }

        private bool IsFilteringState(CompletionItemType type)
        {
            bool result = false;

            if (type == CompletionItemType.Property)
                result = this.isFilteringProperty;
            else if (type == CompletionItemType.Keyword)
                result = this.isFilteringKeyword;
            else if (type == CompletionItemType.Function)
                result = this.isFilteringFunction;

            return result;
        }

        private void SelectTopCandidate()
        {
            double topPriority = -1;
            this.SelectedIndex = -1;

            for(int i=0; i<this.CandidateCollection.Count; i++)
            {
                
                var item = this.CandidateCollection[i];
                double priority = this.similarity.SimilarityValue(item.ItemName, this.inputString);
                if (priority > 0)
                {
                    if (priority > topPriority)
                    {
                        topPriority = priority;
                        this.SelectedIndex = i;
                    }
                }
            }
        }

        public void Clear()
        {
            this.IsFilteringProperty = false;
            this.IsFilteringNamespace = false;
            this.IsFilteringKeyword = false;
            this.IsFilteringFunction = false;
            this.IsFilteringEnum = false;
            this.IsFilteringCodeSnipp = false;
        }

        public void Up()
        {
            if (this.selectedIndex <= 0) return;
            if (this.AvailableCollection.Count == 0) return;

            this.SelectedIndex--;
        }

        public void Down()
        {
            if (this.selectedIndex >= this.AvailableCollection.Count - 1) return;

            this.SelectedIndex++;
        }

        public void LoadAvailableCollection()
        {
            this.AvailableCollection.Clear();
            foreach (var item in this.TotalCollection) this.AvailableCollection.Add(item);
        }

        public void OnInputStringChanged()
        {
            if(this.inputString.Length == 1)
            {
                this.CandidateCollection.Clear();
                foreach (var item in this.AvailableCollection) this.CandidateCollection.Add(item);
            }
            else if(this.inputString.Length > 1)
            {
                this.CandidateCollection.Clear();

                foreach(var item in this.AvailableCollection)
                {
                    if (this.similarity.SimilarityValue(item.ItemName, this.inputString) > 0) this.CandidateCollection.Add(item);
                }

                if(this.CandidateCollection.Count == 0)
                {
                    foreach (var item in this.AvailableCollection) this.CandidateCollection.Add(item);
                }
            }

            if (this.CandidateCollection.Count <= 0) return;

            this.filteredCandidate = new CompletionItem[this.CandidateCollection.Count];
            this.CandidateCollection.CopyTo(this.filteredCandidate, 0);

            // if filtering state
            if (this.IsFilteringAllDisable() == false) this.OnFilteringChanged();
            else this.SelectTopCandidate();
        }

        public void OnFilteringChanged()
        {
            if (this.IsFilteringAllDisable())
            {
                this.OnInputStringChanged();
                return;
            }

            this.CandidateCollection.Clear();
            foreach(var item in this.filteredCandidate)
            {
                if (this.IsFilteringState(item.ItemType) == false) continue;

                this.CandidateCollection.Add(item);
            }

            this.SelectTopCandidate();
        }
    }


    public class CompletionItemComparer : IComparer<CompletionItem>
    {
        public int Compare(CompletionItem x, CompletionItem y)
        {
            // TODO: Handle x or y being null, or them not having names
            return x.ItemName.CompareTo(y.ItemName);
        }
    }
}
