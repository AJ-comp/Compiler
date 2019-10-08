using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Parse.WpfControls.Algorithms;
using Parse.WpfControls.Properties;
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
        private SortedSet<CompletionItem> totalCollection = new SortedSet<CompletionItem>(new CompletionItemComparer());
        private HashSet<CompletionItem> availableCollection = new HashSet<CompletionItem>();
        private ObservableCollection<CodeContentInfo> codeContents = new ObservableCollection<CodeContentInfo>();

        public ObservableCollection<CompletionItem> CandidateCollection { get; private set; } = new ObservableCollection<CompletionItem>();

        public CodeContentInfo PropertyInfo { get; } = new CodeContentInfo("property.png", Resources.Property_, CompletionItemType.Property);
        public CodeContentInfo KeywordInfo { get; } = new CodeContentInfo("keyword.png", Resources.Keyword_, CompletionItemType.Keyword);
        public CodeContentInfo EnumInfo { get; } = new CodeContentInfo("enum.png", Resources.Enumerate, CompletionItemType.Enum);
        public CodeContentInfo CodeSnippInfo { get; } = new CodeContentInfo("codesnipp.png", Resources.CodeSnipp, CompletionItemType.CodeSnipp);
        public CodeContentInfo NamespaceInfo { get; } = new CodeContentInfo("namespace.png", Resources.Namespace, CompletionItemType.Namespace);
        public CodeContentInfo FunctionInfo { get; } = new CodeContentInfo("function.png", Resources.Function_, CompletionItemType.Function);
        public CodeContentInfo EventInfo { get; } = new CodeContentInfo("event.png", Resources.Event, CompletionItemType.Event);
        public CodeContentInfo DelegateInfo { get; } = new CodeContentInfo("delegate.png", Resources.Delegate, CompletionItemType.Delegate);
        public CodeContentInfo ClassInfo { get; } = new CodeContentInfo("class.png", Resources.Class_, CompletionItemType.Class);
        public CodeContentInfo StructInfo { get; } = new CodeContentInfo("struct.png", Resources.Structure, CompletionItemType.Struct);
        public CodeContentInfo InterfaceInfo { get; } = new CodeContentInfo("interface.png", Resources.Interface_, CompletionItemType.Interface);

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

            this.codeContents.Add(this.PropertyInfo);
            this.codeContents.Add(this.KeywordInfo);
            this.codeContents.Add(this.EnumInfo);
            this.codeContents.Add(this.CodeSnippInfo);
            this.codeContents.Add(this.NamespaceInfo);
            this.codeContents.Add(this.FunctionInfo);
            this.codeContents.Add(this.EventInfo);
            this.codeContents.Add(this.DelegateInfo);
            this.codeContents.Add(this.ClassInfo);
            this.codeContents.Add(this.StructInfo);
            this.codeContents.Add(this.InterfaceInfo);

            foreach (var item in this.codeContents)
                item.PropertyChanged += ((s, e) => this.OnFilteringChanged());
        }

        private bool IsFilteringAllDisable()
        {
            foreach(var item in this.codeContents)
            {
                if (item.IsFiltering) return false;
            }

            return true;
        }

        private bool IsFilteringState(CompletionItemType type)
        {
            bool result = false;

            foreach(var item in this.codeContents)
            {
                if(item.ItemType == type)
                {
                    result = item.IsFiltering;
                    break;
                }
            }

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
            foreach (var item in this.codeContents) item.IsFiltering = false;
        }

        public void Up()
        {
            if (this.selectedIndex <= 0) return;
            if (this.availableCollection.Count == 0) return;

            this.SelectedIndex--;
        }

        public void Down()
        {
            if (this.selectedIndex >= this.availableCollection.Count - 1) return;

            this.SelectedIndex++;
        }

        public void LoadAvailableCollection()
        {
            this.availableCollection.Clear();
            foreach (var item in this.totalCollection) this.availableCollection.Add(item);
        }

        public void OnInputStringChanged()
        {
            if(this.inputString.Length == 1)
            {
                this.CandidateCollection.Clear();
                foreach (var item in this.availableCollection) this.CandidateCollection.Add(item);
            }
            else if(this.inputString.Length > 1)
            {
                this.CandidateCollection.Clear();

                foreach(var item in this.availableCollection)
                {
                    if (this.similarity.SimilarityValue(item.ItemName, this.inputString) > 0) this.CandidateCollection.Add(item);
                }

                if(this.CandidateCollection.Count == 0)
                {
                    foreach (var item in this.availableCollection) this.CandidateCollection.Add(item);
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

        public void AddCollection(CompletionItemType type, string name)
        {
            string imgSrc = string.Empty;

            if (type == CompletionItemType.Property) imgSrc = PropertyInfo.ImgSrc;
            else if (type == CompletionItemType.Keyword) imgSrc = KeywordInfo.ImgSrc;
            else if (type == CompletionItemType.Enum) imgSrc = EnumInfo.ImgSrc;
            else if (type == CompletionItemType.CodeSnipp) imgSrc = CodeSnippInfo.ImgSrc;
            else if (type == CompletionItemType.Function) imgSrc = FunctionInfo.ImgSrc;
            else if (type == CompletionItemType.Namespace) imgSrc = NamespaceInfo.ImgSrc;
            else if (type == CompletionItemType.Event) imgSrc = EventInfo.ImgSrc;
            else if (type == CompletionItemType.Delegate) imgSrc = DelegateInfo.ImgSrc;
            else if (type == CompletionItemType.Class) imgSrc = ClassInfo.ImgSrc;
            else if (type == CompletionItemType.Struct) imgSrc = StructInfo.ImgSrc;
            else if (type == CompletionItemType.Interface) imgSrc = InterfaceInfo.ImgSrc;

            this.totalCollection.Add(new CompletionItem() { ImageSource = imgSrc, ItemName = name, ItemType = type });
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
