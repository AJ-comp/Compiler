using System;
using System.ComponentModel;

namespace Parse.WpfControls.SyntaxEditorComponents.Models
{
    public class CodeContentInfo : INotifyPropertyChanged
    {
        public string ImgSrc { get; }
        public string Explain { get; }
        public CompletionItemType ItemType { get; }

        private bool isFiltering;
        public bool IsFiltering
        {
            get => this.isFiltering;
            set
            {
                this.isFiltering = value;
                this.OnPropertyChanged("IsFiltering");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public CodeContentInfo(string imgName, string explain, CompletionItemType itemType)
        {
            this.ImgSrc = "pack://application:,,,/Parse.WpfControls;component/Resources/" + imgName;
            this.Explain = explain;
            this.ItemType = itemType;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
