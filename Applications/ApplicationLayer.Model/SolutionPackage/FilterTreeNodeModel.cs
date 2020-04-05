using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ApplicationLayer.Models.SolutionPackage
{
    public class FilterTreeNodeModel : TreeNodeModel
    {
        private string filterNameRecentSaved = string.Empty;
        private ObservableCollection<FilterTreeNodeModel> filtersWhenLoad = new ObservableCollection<FilterTreeNodeModel>();
        private ObservableCollection<FileTreeNodeModel> filesWhenLoad = new ObservableCollection<FileTreeNodeModel>();

        [XmlIgnore]
        public string FilterName { get; set; }

        [XmlIgnore]
        public ObservableCollection<FilterTreeNodeModel> Filters { get; private set; } = new ObservableCollection<FilterTreeNodeModel>();

        [XmlIgnore]
        public ObservableCollection<FileTreeNodeModel> Files { get; private set; } = new ObservableCollection<FileTreeNodeModel>();

        [XmlIgnore]
        public List<FilterFileTreeNodeModel> ToFilterFileTreeNodeModel
        {
            get
            {
                List<FilterFileTreeNodeModel> result = new List<FilterFileTreeNodeModel>();

                foreach (var item in this.Files)
                {
                    FilterFileTreeNodeModel curNode = new FilterFileTreeNodeModel(this.FilterName, item.Path, item.FileName);

                    result.Add(curNode);
                }

                foreach (var item in this.Filters)
                {
                    foreach(var childItem in item.ToFilterFileTreeNodeModel)
                    {
                        string filterFullPath = Path.Combine(this.FilterName, childItem.FilterPath);
                        FilterFileTreeNodeModel curNode = new FilterFileTreeNodeModel(filterFullPath, childItem.Path, childItem.FileName);

                        result.Add(curNode);
                    }
                }

                if (this.Filters.Count == 0 && this.Files.Count == 0)
                    result.Add(new FilterFileTreeNodeModel(this.FilterName, string.Empty, string.Empty));

                return result;
            }
        }

        public override bool IsChanged
        {
            get
            {
                if (filterNameRecentSaved != FilterName) return true;
                if (Files.Count != filesWhenLoad.Count) return true;
                if (Filters.Count != filtersWhenLoad.Count) return true;

                bool result = false;
                Parallel.For(0, Files.Count, (i,loopOption) =>
                {
                    if (Files.Contains(filesWhenLoad[i]) == false)
                    {
                        result = true;
                        loopOption.Stop();
                    }
                });
                if (result) return true;

                foreach (var filter in Filters)
                    if (filter.IsChanged) return true;

                return false;
            }
        }

        public override string DisplayName => FilterName;

        public override string FullOnlyPath => Parent.FullOnlyPath;

        public FilterTreeNodeModel(string name)
        {
            this.filterNameRecentSaved = name;
            this.Filters.CollectionChanged += TreeNodeModel.CollectionChanged;
            this.Files.CollectionChanged += TreeNodeModel.CollectionChanged;

            this.SyncWithLoadValue();
        }

        public override void SyncWithLoadValue()
        {
            FilterName = filterNameRecentSaved;

            Filters = filtersWhenLoad;
            Files = filesWhenLoad;
        }

        public override void SyncWithCurrentValue()
        {
            filterNameRecentSaved = FilterName;

            filtersWhenLoad = Filters;
            filesWhenLoad = Files;
        }
    }
}
