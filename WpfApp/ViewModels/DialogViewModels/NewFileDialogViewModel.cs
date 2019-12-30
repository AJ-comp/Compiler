using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using WpfApp.Models;

namespace WpfApp.ViewModels.DialogViewModels
{
    public class NewFileDialogViewModel : DialogViewModel
    {
        public int CurSelected { get; set; } = 0;
        public ObservableCollection<Document> NewFileDataCollection { get; } = new ObservableCollection<Document>();

        public NewFileDialogViewModel()
        {
            this.NewFileDataCollection.Add(new Document("pack://application:,,,/WpfApp;component/Resources/typec24.png", Properties.Resources.MiniCFile, Properties.Resources.MiniCFileExplain));
        }

        public event EventHandler<Document> CreateRequest;

        protected override void OnCreate(Action action)
        {
            this.CreateRequest?.Invoke(this, this.NewFileDataCollection[this.CurSelected]);
            base.OnCreate(action);
        }
    }
}
