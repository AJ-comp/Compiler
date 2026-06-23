using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.ViewModels.Interfaces
{
    public interface IPathSearchable
    {
        string Path { get; set; }
        RelayCommand<Action<string>> SearchCommand { get; }
    }
}
