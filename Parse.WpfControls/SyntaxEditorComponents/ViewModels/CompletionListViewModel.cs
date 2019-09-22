using GalaSoft.MvvmLight.Command;
using Parse.WpfControls.Abstracts;
using Parse.WpfControls.SyntaxEditorComponents.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Parse.WpfControls.SyntaxEditorComponents.ViewModels
{
    public class CompletionListViewModel : ViewModelBase
    {
        public ObservableCollection<CompletionItem> TotalCollection { get; } = new ObservableCollection<CompletionItem>();
        public ObservableCollection<CompletionItem> AvailableCollection { get; } = new ObservableCollection<CompletionItem>();

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

        public void OnInputStringChanged()
        {
            // Show the all available completion item when the length of the input string is 1.
            if(this.InputString.Length == 1)
            {
                foreach (var item in this.TotalCollection) this.AvailableCollection.Add(item);

                int minFindIndex = 0xff;
                for (int i = 0; i < this.AvailableCollection.Count; i++)
                {
                    var item = this.AvailableCollection[i];
                    int findIndex = item.ItemName.IndexOf(this.InputString);
                    if (findIndex >= 0 && findIndex < minFindIndex)
                    {
                        minFindIndex = findIndex;
                        this.SelectedIndex = i;
                    }
                }
            }
            else if(this.inputString.Length > 1)
            {
                foreach (var item in this.TotalCollection) this.AvailableCollection.Add(item);
            }
            else
            {
                this.AvailableCollection.Clear();
            }
        }

        public void Clear()
        {
            this.TotalCollection.Clear();
            this.AvailableCollection.Clear();
        }
    }
}
