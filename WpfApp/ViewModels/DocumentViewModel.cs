using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using WpfApp.Models;

namespace WpfApp.ViewModels
{
    public class DocumentViewModel : ViewModelBase
    {
        public event EventHandler RequestClose;
    }
}
