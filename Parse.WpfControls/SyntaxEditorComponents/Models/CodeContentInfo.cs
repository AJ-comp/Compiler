using System.ComponentModel;

namespace Parse.WpfControls.SyntaxEditorComponents.Models
{
    public enum CompletionItemType { Field, Keyword, Property, Enum, Namespace, CodeSnipp, Function, Event, Delegate, Class, Struct, Interface };

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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
