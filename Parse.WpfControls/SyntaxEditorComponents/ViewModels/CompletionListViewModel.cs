using GalaSoft.MvvmLight.Command;
using Parse.WpfControls.Abstracts;
using Parse.WpfControls.Algorithms;
using Parse.WpfControls.SyntaxEditorComponents.Models;
using Parse.WpfControls.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;

namespace Parse.WpfControls.SyntaxEditorComponents.ViewModels
{
    public class CompletionListViewModel : ViewModelBase
    {
        private ISimilarityComparison similarity = new VSLikeSimilarityComparison();

        public SortedSet<CompletionItem> TotalCollection { get; } = new SortedSet<CompletionItem>(new CompletionItemComparer());
        public HashSet<CompletionItem> AvailableCollection { get; private set; } = new HashSet<CompletionItem>();
        public ObservableCollection<CompletionItem> CandidateCollection { get; private set; } = new ObservableCollection<CompletionItem>();

//        RoutedEvent SelectedItem

        private RelayCommand<bool> doubleClickCmd;
        public RelayCommand<bool> DoubleClickCmd
        {
            get => this.doubleClickCmd ?? (this.doubleClickCmd = new RelayCommand<bool>(DoubleClickAction));
        }

        private int selectedIndex = 0;
        public int SelectedIndex
        {
            get => this.selectedIndex;
            set
            {
                this.selectedIndex = value;
                this.OnPropertyChanged("SelectedIndex");
            }
        }

        private string inputString = string.Empty;
        public string InputString
        {
            get => this.inputString;
            set
            {
                this.inputString = value;
                this.OnPropertyChanged("InputString");
                this.OnInputStringChanged();
            }
        }

        private void DoubleClickAction(bool bDoubleClick)
        {

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
