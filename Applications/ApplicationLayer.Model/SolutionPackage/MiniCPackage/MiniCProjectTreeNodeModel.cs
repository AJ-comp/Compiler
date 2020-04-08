using Parse.BackEnd.Target;
using Parse.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Xml.Serialization;
using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.Models.SolutionPackage.MiniCPackage
{
    [XmlInclude(typeof(FilterFileTreeNodeModel))]
    [XmlRoot("MiniC Project")]
    public class MiniCProjectTreeNodeModel : ProjectTreeNodeModel
    {
        private ProjectProperty debugConfigureRecentSaved = new ProjectProperty();
        private ProjectProperty releaseConfigureRecentSaved = new ProjectProperty();

        private FilterTreeNodeModel references = new FilterTreeNodeModel(CommonResource.References);
        private FilterTreeNodeModel outerDependenies = new FilterTreeNodeModel(CommonResource.ExternDependency);

        private ObservableCollection<FilterTreeNodeModel> filters = new ObservableCollection<FilterTreeNodeModel>();
        private ObservableCollection<FileTreeNodeModel> files = new ObservableCollection<FileTreeNodeModel>();

        [XmlElement("Debug Property")]
        public ProjectProperty DebugConfigure { get; set; } = new ProjectProperty();

        [XmlElement("Release Property")]
        public ProjectProperty ReleaseConfigure { get; set; } = new ProjectProperty();

        [XmlIgnore]
        public FilterTreeNodeModel References
        {
            get => references;
            set
            {
                references = value;
                OnPropertyChanged(nameof(References));
            }
        }
        [XmlIgnore]
        public FilterTreeNodeModel OuterDependencies
        {
            get => outerDependenies;
            set
            {
                outerDependenies = value;
                OnPropertyChanged(nameof(OuterDependencies));
            }
        }

        [XmlIgnore]
        public ObservableCollection<FilterTreeNodeModel> Filters => filters;

        [XmlIgnore]
        public ObservableCollection<FileTreeNodeModel> Files => files;

        [XmlElement("FilterAndFiles")]
        public List<FilterFileTreeNodeModel> FilterFiles { get; set; } = new List<FilterFileTreeNodeModel>();

        [XmlArrayItem("IncludePath")]
        public StringCollection ReferencePaths
        {
            get
            {
                StringCollection result = new StringCollection();

                foreach(var item in References.Files)
                {
                    FileTreeNodeModel referenceFile = item as FileTreeNodeModel;

                    result.Add(referenceFile.PathWithFileName);
                }

                return result;
            }
            set
            {
                foreach(var item in value)
                {
                    string path = System.IO.Path.GetDirectoryName(item);
                    string fileName = System.IO.Path.GetFileName(item);
                    References.AddFile(new FileTreeNodeModel(path, fileName));
                }
            }
        }

        [XmlArrayItem("IncludePath")]
        public StringCollection OuterDependencyPaths
        {
            get
            {
                StringCollection result = new StringCollection();

                foreach (var item in OuterDependencies.Files)
                {
                    FileTreeNodeModel outerDepFile = item as FileTreeNodeModel;

                    result.Add(outerDepFile.PathWithFileName);
                }

                return result;
            }
            set
            {
                foreach (var item in value)
                {
                    string path = System.IO.Path.GetDirectoryName(item);
                    string fileName = System.IO.Path.GetFileName(item);
                    OuterDependencies.AddFile(new FileTreeNodeModel(path, fileName));
                }
            }
        }

        public override string ProjectType => LanguageExtensions.MiniC;

        public override bool IsChanged
        {
            get
            {
                if (debugConfigureRecentSaved.Equals(DebugConfigure) == false) return true;
                if (releaseConfigureRecentSaved.Equals(ReleaseConfigure) == false) return true;

                List<FilterFileTreeNodeModel> dataToCompare = new List<FilterFileTreeNodeModel>();
                foreach (var filter in this.filters)
                    dataToCompare.AddRange(filter.ToFilterFileTreeNodeModel);

                foreach(var file in this.files)
                {
                    FilterFileTreeNodeModel curNode = new FilterFileTreeNodeModel(string.Empty, file.Path, file.FileName);

                    dataToCompare.Add(curNode);
                }

                if (this.FilterFiles.IsEqual(dataToCompare) == false) return true;

                return false;
            }
        }

        public MiniCProjectTreeNodeModel() : base(string.Empty, string.Empty)
        {
        }

        public MiniCProjectTreeNodeModel(string path, string projName, Target target) : base(path, projName + string.Format(".{0}proj", LanguageExtensions.MiniC))
        {
            this.DebugConfigure.Target = target?.Name;
            this.ReleaseConfigure.Target = target?.Name;

            this.SyncWithCurrentValue();
        }

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

        public override void RemoveChild(TreeNodeModel nodeToRemove)
        {
            if (nodeToRemove is FilterTreeNodeModel)
                this.filters.Remove(nodeToRemove as FilterTreeNodeModel);
            else if (nodeToRemove is FileTreeNodeModel)
                this.files.Remove(nodeToRemove as FileTreeNodeModel);
        }

        public override void SyncWithLoadValue()
        {
            this.LoadElement();
        }

        public override void SyncWithCurrentValue()
        {
            FilterFiles.Clear();
            foreach (var item in Filters)
            {
                foreach (var ffitem in item.ToFilterFileTreeNodeModel) FilterFiles.Add(ffitem);
            }
        }

        public override void LoadElement()
        {
            foreach (var item in FilterFiles)
            {
                var filterDataTree = item.FilterDataTree;
                if (filterDataTree != null)
                {
                    this.AddFilter(filterDataTree);
                    continue;
                }

                var notFilterDataTree = item.NotFilterDataTree;
                if (notFilterDataTree != null)
                {
                    this.AddFile(notFilterDataTree);
                    continue;
                }
            }
        }

        public override void Save()
        {
            Directory.CreateDirectory(this.FullOnlyPath);
            string fullPath = System.IO.Path.Combine(this.FullOnlyPath, this.FileName);

            Type type = typeof(ProjectTreeNodeModel);
            if (System.IO.Path.GetExtension(this.FileName) == string.Format(".{0}proj", LanguageExtensions.MiniC))
                type = typeof(MiniCProjectTreeNodeModel);

            using (StreamWriter wr = new StreamWriter(fullPath))
            {
                XmlSerializer xs = new XmlSerializer(type);
                xs.Serialize(wr, this);
            }
        }
    }
}
