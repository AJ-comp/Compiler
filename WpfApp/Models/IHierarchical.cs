using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp.Models
{
    interface IHierarchical<T>
    {
        ObservableCollection<T> Items { get; }
    }
}
