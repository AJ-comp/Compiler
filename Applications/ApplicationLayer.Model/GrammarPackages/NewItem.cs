using System.ComponentModel;

namespace ApplicationLayer.Models.GrammarPackages
{
    public class NewItem : INotifyPropertyChanged
    {
        public string ImagePath { get; }
        public string ItemType { get; }
        public string ItemExplain { get; }
        public string ItemDetailExplain { get; }

        private string itemName;
        public string ItemName
        {
            get => itemName;
            set
            {
                itemName = value;
                OnPropertyChanged("DefaultFileName");
            }
        }

        public NewItem(string imagePath, string itemType, string itemExplain, string itemDetailExplain, string itemName)
        {
            ImagePath = imagePath;
            ItemType = itemType;
            ItemExplain = itemExplain;
            ItemDetailExplain = itemDetailExplain;
            ItemName = itemName;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
