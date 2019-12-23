using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using WpfApp.Models;

namespace WpfApp.ViewModels.DocumentTypeViewModels
{
    public class DocumentViewModel : ViewModelBase
    {
        public DocumentViewModel(string title)
        {
            Title = title;
        }

        public string Title { get; }

        public event EventHandler RequestClose;
    }
}
