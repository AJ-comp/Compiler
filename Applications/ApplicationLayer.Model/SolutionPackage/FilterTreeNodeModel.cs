using ApplicationLayer.Common;
using ApplicationLayer.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ApplicationLayer.Models.SolutionPackage
{
    public class FilterTreeNodeModel : TreeNodeModel, IManagedable
    {
        /********************************************************************************************
         * property section
         ********************************************************************************************/
        [XmlIgnore] public string FilterName { get; set; }

        [XmlIgnore] public List<FilterFileTreeNodeModel> ToFilterFileTreeNodeModel
        {
            get
            {
                List<FilterFileTreeNodeModel> result = new List<FilterFileTreeNodeModel>();

                foreach (var child in Children)
                {
                    if(child is FilterTreeNodeModel)
                    {
                        var filter = child as FilterTreeNodeModel;
                        foreach (var childItem in filter.ToFilterFileTreeNodeModel)
                        {
                            string filterFullPath = Path.Combine(this.FilterName, childItem.FilterPath);
                            FilterFileTreeNodeModel curNode = new FilterFileTreeNodeModel(filterFullPath, childItem.Path, childItem.FileName);

                            result.Add(curNode);
                        }
                    }
                    else if(child is FileTreeNodeModel)
                    {
                        var file = child as FileTreeNodeModel;
                        FilterFileTreeNodeModel curNode = new FilterFileTreeNodeModel(this.FilterName, file.Path, file.FileName);

                        result.Add(curNode);
                    }
                }

                if (this.Children.Count == 0)
                    result.Add(new FilterFileTreeNodeModel(this.FilterName, string.Empty, string.Empty));

                return result;
            }
        }

        public StringCollection ToPathString
        {
            get
            {
                StringCollection result = new StringCollection();

                Parallel.For(0, Children.Count, (index) =>
                {
                    var child = Children[index];
                    if(child is FilterTreeNodeModel)
                    {
                        var filter = child as FilterTreeNodeModel;
                        foreach (var pathString in filter.ToPathString)
                            result.Add(Path.Combine(this.FilterName, pathString));
                    }
                    else if(child is FileTreeNodeModel)
                    {
                        var file = child as FileTreeNodeModel;
                        result.Add(Path.Combine(this.FilterName, file.FileName));
                    }
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
            this.Children.Add(item);
        }

        public void AddFile(FileTreeNodeModel item)
        {
            item.Parent = this;
            this.Children.Add(item);
        }



        /********************************************************************************************
         * override method section
         ********************************************************************************************/
        public override void RemoveChild(TreeNodeModel nodeToRemove)
        {
            if (nodeToRemove is FilterTreeNodeModel)
                this.Children.Remove(nodeToRemove as FilterTreeNodeModel);
            else if (nodeToRemove is FileTreeNodeModel)
                this.Children.Remove(nodeToRemove as FileTreeNodeModel);
        }
    }
}
