using ApplicationLayer.Models.SolutionPackage;
using System.ComponentModel;

namespace ApplicationLayer.Models
{
    public class Document : INotifyPropertyChanged
    {
        public string ImageSource { get; }
        public string ItemType { get; }
        public string Explain { get; }

        private string detailExplain;
        public string DetailExplain
        {
            get => detailExplain;
            set
            {
                detailExplain = value;
                OnPropertyChanged("DetailExplain");
            }
        }

        private string itemName;
        public string ItemName
        {
            get => itemName;
            set
            {
                itemName = value;
                OnPropertyChanged("ItemName");
            }
        }
        public string Data { get; }

        public Document(string imageSource, string itemType, string explain, string detailExplain, string itemName)
        {
            ImageSource = imageSource;
            ItemType = itemType;
            Explain = explain;
            DetailExplain = detailExplain;
            ItemName = itemName;
        }


        public FileStruct FileStruct
        {
            get
            {
                FileStruct result = new FileStruct()
                {
                    ImageSource = ImageSource,
                    FullName = ItemName
                };

                return result;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
