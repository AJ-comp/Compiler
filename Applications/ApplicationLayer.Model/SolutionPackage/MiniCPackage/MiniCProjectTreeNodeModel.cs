using Parse.BackEnd.Target;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
        public ObservableCollection<FilterTreeNodeModel> Filters { get; } = new ObservableCollection<FilterTreeNodeModel>();
        [XmlIgnore]
        public ObservableCollection<FileTreeNodeModel> Files { get; } = new ObservableCollection<FileTreeNodeModel>();

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
                    References.Files.Add(new FileTreeNodeModel(path, fileName));
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
                    OuterDependencies.Files.Add(new FileTreeNodeModel(path, fileName));
                }
            }
        }

        public override string ProjectType => LanguageExtensions.MiniC;

        public override bool IsChanged
        {
            get
            {
                if (base.IsChanged) return true;

                if (debugConfigureRecentSaved.Equals(DebugConfigure) == false) return true;
                if (releaseConfigureRecentSaved.Equals(ReleaseConfigure) == false) return true;
                if (references.IsChanged) return true;
                if (outerDependenies.IsChanged) return true;

                return false;
            }
        }

        public MiniCProjectTreeNodeModel() : base(string.Empty, string.Empty) 
        {
            this.Filters.CollectionChanged += TreeNodeModel.CollectionChanged;
            this.Files.CollectionChanged += TreeNodeModel.CollectionChanged;
        }

        public MiniCProjectTreeNodeModel(string path, string projName, Target target) : base(path, projName + string.Format(".{0}proj", LanguageExtensions.MiniC))
        {
            this.DebugConfigure.Target = target?.Name;
            this.ReleaseConfigure.Target = target?.Name;

            this.Filters.CollectionChanged += TreeNodeModel.CollectionChanged;
            this.Files.CollectionChanged += TreeNodeModel.CollectionChanged;

            this.SyncWithCurrentValue();
        }

        public override void SyncWithLoadValue()
        {
            base.SyncWithLoadValue();

            this.LoadElement();
        }

        public override void SyncWithCurrentValue()
        {
            base.SyncWithCurrentValue();

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
                    Filters.Add(filterDataTree);
                    continue;
                }

                var notFilterDataTree = item.NotFilterDataTree;
                if (notFilterDataTree != null)
                {
                    Files.Add(notFilterDataTree);
                    continue;
                }
            }
        }
    }
}
