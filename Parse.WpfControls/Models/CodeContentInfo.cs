using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace Parse.WpfControls.Models
{
    public enum CompletionItemType { Field, Keyword, Property, Enum, Namespace, CodeSnipp, Function, Event, Delegate, Class, Struct, Interface };

    public class CodeContentInfo : INotifyPropertyChanged
    {
        public string ImgSrc { get; }
        public string Explain { get; }
        public CompletionItemType ItemType { get; }
        public Enum Type { get; }

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

        public CodeContentInfo(string imgName, string explain, Enum itemType)
        {
            this.ImgSrc = "pack://application:,,,/Parse.WpfControls;component/Resources/" + imgName;
            this.Explain = explain;
            this.Type = itemType;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }


    public class Model
    {
        public Model(Enum type, string item)
        {
            Type = type;
            Item = item;
        }

        public Enum Type { get; }
        public string Item { get; }
    }

    public class KeyData
    {
        public KeyData() { }

        public KeyData(string activeImgSource, string inActiveImgSource, string toolTipExplain)
        {
            ActiveImgSource = activeImgSource;
            InActiveImgSource = inActiveImgSource;
            ToolTipData = toolTipExplain;
        }

        public string ActiveImgSource { get; } = string.Empty;
        public string InActiveImgSource { get; } = string.Empty;
        public string ToolTipData { get; } = string.Empty;
    }

    public class ItemData
    {
        public ItemData(Enum type, string item, string toolTipData)
        {
            Type = type;
            Item = item;
            ToolTipData = toolTipData;
        }

        public BitmapImage ImageSource { get; set; }
        public List<UInt32> MatchedIndexes { get; set; }

        public Enum Type { get; }
        public string Item { get; }
        public string ToolTipData { get; }
    }
}
