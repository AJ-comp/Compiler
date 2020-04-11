using ApplicationLayer.Common;
using ApplicationLayer.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ApplicationLayer.Models.SolutionPackage
{
    public class FilterTreeNodeModel : TreeNodeModel, IManagedable
    {
        /********************************************************************************************
         * private field section
         ********************************************************************************************/
        private ObservableCollection<FilterTreeNodeModel> filters = new ObservableCollection<FilterTreeNodeModel>();
        private ObservableCollection<FileTreeNodeModel> files = new ObservableCollection<FileTreeNodeModel>();



        /********************************************************************************************
         * property section
         ********************************************************************************************/
        [XmlIgnore] public string FilterName { get; set; }

        [XmlIgnore] public ObservableCollection<FilterTreeNodeModel> Filters => filters;

        [XmlIgnore] public ObservableCollection<FileTreeNodeModel> Files => files;

        [XmlIgnore] public List<FilterFileTreeNodeModel> ToFilterFileTreeNodeModel
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

        public StringCollection ToPathString
        {
            get
            {
                StringCollection result = new StringCollection();

                Parallel.For(0, Files.Count, (index) =>
                {
                    var file = Files[index];
                        result.Add(Path.Combine(this.FilterName, file.FileName));
                });

                Parallel.For(0, Filters.Count, (index) =>
                {
                    var filter = Filters[index];
                    foreach (var pathString in filter.ToPathString)
                        result.Add(Path.Combine(this.FilterName, pathString));
                });

                return result;
            }
        }



        /********************************************************************************************
         * override property section
         ********************************************************************************************/
        public override string DisplayName
        {
            get => FilterName;
            set => FilterName = value;
        }

        public override string FullOnlyPath => Parent.FullOnlyPath;



        /********************************************************************************************
         * interface property field section
         ********************************************************************************************/
        public IManagableElements ManagerTree
        {
            get
            {
                TreeNodeModel current = this;

                while (true)
                {
                    if (current == null) return null;
                    else if (current is SolutionTreeNodeModel) return current as SolutionTreeNodeModel;
                    else if (current is ProjectTreeNodeModel) return current as ProjectTreeNodeModel;

                    current = current.Parent;
                }
            }
        }



        /********************************************************************************************
         * event handler section
         ********************************************************************************************/
        public event EventHandler<FileChangedEventArgs> Changed;



        /********************************************************************************************
         * constructor section
         ********************************************************************************************/
        public FilterTreeNodeModel(string name)
        {
            this.FilterName = name;
            this.IsEditable = true;
        }



        /********************************************************************************************
         * public method section
         ********************************************************************************************/
        public void AddFilter(FilterTreeNodeModel item)
        {
            item.Parent = this;
            this.filters.Add(item);
        }

        public void AddFile(FileTreeNodeModel item)
        {
            item.Parent = this;
            this.files.Add(item);
        }



        /********************************************************************************************
         * override method section
         ********************************************************************************************/
        public override void RemoveChild(TreeNodeModel nodeToRemove)
        {
            if (nodeToRemove is FilterTreeNodeModel)
                this.filters.Remove(nodeToRemove as FilterTreeNodeModel);
            else if (nodeToRemove is FileTreeNodeModel)
                this.files.Remove(nodeToRemove as FileTreeNodeModel);
        }
    }
}
