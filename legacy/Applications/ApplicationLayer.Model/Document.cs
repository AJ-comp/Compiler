using ApplicationLayer.Models.SolutionPackage;
using System.ComponentModel;

namespace ApplicationLayer.Models
{
    public enum DocumentType { MiniCHeader, MiniCSource };

    public class Document : INotifyPropertyChanged
    {
        private string itemName;
        private string detailExplain;
        private string data;



        public DocumentType DocumentType { get; }
        public string ItemType { get; }
        public string Explain { get; }
        public string DetailExplain
        {
            get => detailExplain;
            private set
            {
                detailExplain = value;
                OnPropertyChanged(nameof(DetailExplain));
            }
        }

        public string ItemName
        {
            get => itemName;
            set
            {
                itemName = value;
                OnPropertyChanged(nameof(ItemName));
            }
        }

        public string Data
        {
            get => data;
            set
            {
                data = value;
                OnPropertyChanged(nameof(Data));
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;



        public Document(DocumentType documentType, string itemType, string explain, string detailExplain, string itemName)
        {
            DocumentType = documentType;
            ItemType = itemType;
            Explain = explain;
            DetailExplain = detailExplain;
            ItemName = itemName;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
