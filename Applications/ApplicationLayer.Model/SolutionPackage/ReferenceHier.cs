using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Xml.Serialization;

namespace ApplicationLayer.Models.SolutionPackage
{
    [XmlInclude(typeof(ReferenceFileStruct))]
    public class ReferenceHier : HierarchicalData
    {
        public ObservableCollection<ReferenceFileStruct> Items { get; } = new ObservableCollection<ReferenceFileStruct>();

        public override string DisplayName => this.NameWithoutExtension;

        private ReferenceHier() : this(string.Empty, string.Empty)
        {
        }

        public ReferenceHier(string curOpath, string fullName) : base(curOpath, fullName)
        {
            this.Items.CollectionChanged += ReferenceFiles_CollectionChanged;

            this.ToChangeDisplayName = this.DisplayName;
        }

        private void ReferenceFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < e.NewItems?.Count; i++)
            {
                ReferenceFileStruct item = e.NewItems[i] as ReferenceFileStruct;
                item.Parent = this;
            }
        }

        public override void Save()
        {
            // nothing need do
        }

        public override void ChangeDisplayName()
        {
            string extension = Path.GetExtension(this.FullName);

            this.FullName = this.ToChangeDisplayName + "." + extension;
        }
        public override void CancelChangeDisplayName() => this.ToChangeDisplayName = this.FullName;
    }

    public class ReferenceFileStruct : HierarchicalData
    {
        public override string DisplayName => this.NameWithoutExtension;

        private ReferenceFileStruct() : this(string.Empty, string.Empty)
        {
        }

        public ReferenceFileStruct(string curOpath, string fullName) : base(curOpath, fullName)
        {
            this.ToChangeDisplayName = this.DisplayName;
        }

        public override void Save()
        {
            // nothing need do.
        }

        public override void ChangeDisplayName()
        {
            string extension = Path.GetExtension(this.FullName);

            this.FullName = this.ToChangeDisplayName + "." + extension;
        }
        public override void CancelChangeDisplayName() => this.ToChangeDisplayName = this.FullName;
    }
}
