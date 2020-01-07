using System;
using System.Collections.ObjectModel;

namespace WpfApp.Models
{
    public class ClassHierarchyData : IHierarchical<ClassHierarchyData>
    {
        public Type Data { get; set; }

        public ObservableCollection<ClassHierarchyData> Items { get; set; } = new ObservableCollection<ClassHierarchyData>();
    }
}
