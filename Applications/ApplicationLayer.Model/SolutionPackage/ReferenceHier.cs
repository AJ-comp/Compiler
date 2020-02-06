using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Xml.Serialization;

namespace ApplicationLayer.Models.SolutionPackage
{
    [XmlInclude(typeof(ReferenceFileStruct))]
    public class ReferenceHier : HierarchicalData
    {
        public ObservableCollection<ReferenceFileStruct> Items { get; } = new ObservableCollection<ReferenceFileStruct>();

        public ReferenceHier()
        {
            this.Items.CollectionChanged += ReferenceFiles_CollectionChanged;
        }

        private void ReferenceFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < e.NewItems?.Count; i++)
            {
                ReferenceFileStruct item = e.NewItems[i] as ReferenceFileStruct;
                item.Parent = this;
            }
        }
    }

    public class ReferenceFileStruct : HierarchicalData
    {
    }
}
