using ApplicationLayer.Common;
using Parse.BackEnd.Target;
using Parse.Extensions;
using System.Collections.Generic;
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
        /********************************************************************************************
         * private field section
         ********************************************************************************************/
        private ProjectProperty debugConfigureRecentSaved = new ProjectProperty();
        private ProjectProperty releaseConfigureRecentSaved = new ProjectProperty();

        private FilterTreeNodeModel references = new FilterTreeNodeModel(CommonResource.References);
        private FilterTreeNodeModel outerDependenies = new FilterTreeNodeModel(CommonResource.ExternDependency);



        /********************************************************************************************
         * property section [XML ELEMENT]
         ********************************************************************************************/
        [XmlElement("Debug Property")] public ProjectProperty DebugConfigure { get; set; } = new ProjectProperty();
        [XmlElement("Release Property")] public ProjectProperty ReleaseConfigure { get; set; } = new ProjectProperty();
        [XmlElement("FilterAndFiles")] public List<FilterFileTreeNodeModel> FilterFiles { get; set; } = new List<FilterFileTreeNodeModel>();
        [XmlArrayItem("IncludePath")] public StringCollection ReferencePaths
        {
            get
            {
                StringCollection result = new StringCollection();

                foreach (var item in References.Children)
                {
                    FileTreeNodeModel referenceFile = item as FileTreeNodeModel;

                    result.Add(referenceFile.PathWithFileName);
                }

                return result;
            }
            set
            {
                foreach (var item in value)
                {
                    string path = System.IO.Path.GetDirectoryName(item);
                    string fileName = System.IO.Path.GetFileName(item);
                    References.AddFile(FileTreeNodeModel.CreateFileTreeNodeModel(path, fileName));
                }
            }
        }

        [XmlArrayItem("IncludePath")] public StringCollection OuterDependencyPaths
        {
            get
            {
                StringCollection result = new StringCollection();

                foreach (var item in OuterDependencies.Children)
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
                    OuterDependencies.AddFile(FileTreeNodeModel.CreateFileTreeNodeModel(path, fileName));
                }
            }
        }



        /********************************************************************************************
         * property section [XML ignore]
         ********************************************************************************************/
        [XmlIgnore] public FilterTreeNodeModel References
        {
            get => references;
            set
            {
                references = value;
                OnPropertyChanged(nameof(References));
            }
        }

        [XmlIgnore] public FilterTreeNodeModel OuterDependencies
        {
            get => outerDependenies;
            set
            {
                outerDependenies = value;
                OnPropertyChanged(nameof(OuterDependencies));
            }
        }



        /********************************************************************************************
         * override property section
         ********************************************************************************************/
        public override string ProjectType => LanguageExtensions.MiniCSource;

        public override bool IsChanged
        {
            get
            {
                if (debugConfigureRecentSaved.Equals(DebugConfigure) == false) return true;
                if (releaseConfigureRecentSaved.Equals(ReleaseConfigure) == false) return true;

                List<FilterFileTreeNodeModel> dataToCompare = new List<FilterFileTreeNodeModel>();
                foreach(var child in this.Children)
                {
                    if(child is FilterTreeNodeModel)
                    {
                        FilterTreeNodeModel filter = child as FilterTreeNodeModel;
                        dataToCompare.AddRange(filter.ToFilterFileTreeNodeModel);
                    }
                    else if(child is FileTreeNodeModel)
                    {
                        FileTreeNodeModel file = child as FileTreeNodeModel;
                        FilterFileTreeNodeModel curNode = new FilterFileTreeNodeModel(string.Empty, file.Path, file.FileName);

                        dataToCompare.Add(curNode);
                    }
                }

                if (this.FilterFiles.IsEqual(dataToCompare) == false) return true;

                return false;
            }
        }

        public override IEnumerable<FileReferenceInfo> FileReferenceInfos
        {
            get
            {
                List<FileReferenceInfo> result = new List<FileReferenceInfo>();

                foreach (var file in AllFileNodes)
                {
                    if (file is SourceFileTreeNodeModel == false) continue;

                    var cFile = file as SourceFileTreeNodeModel;
                    result.Add(new FileReferenceInfo(FileHelper.ConvertTargetFileName(cFile.FileName)));
                }

                return result;
            }
        }



        /********************************************************************************************
         * constructor section
         ********************************************************************************************/
        public MiniCProjectTreeNodeModel() : base(string.Empty, string.Empty)
        {
            this.references.IsEditable = false;
            this.outerDependenies.IsEditable = false;
        }

        public MiniCProjectTreeNodeModel(string path, string projName, Target target)
            : base(path, projName + string.Format(".{0}proj", LanguageExtensions.MiniCSource))
        {
            this.DebugConfigure.Target = target?.Name;
            this.ReleaseConfigure.Target = target?.Name;

            this.references.IsEditable = false;
            this.outerDependenies.IsEditable = false;

            this.SyncWithCurrentValue();
        }



        /********************************************************************************************
         * public method section
         ********************************************************************************************/
        public void AddFilter(FilterTreeNodeModel item)
        {
            item.Parent = this;
            this._children.Add(item);
        }

        public void AddFile(FileTreeNodeModel item)
        {
            item.Parent = this;
            this._children.Add(item);
        }



        /********************************************************************************************
         * override method section
         ********************************************************************************************/
        public override void RemoveChild(TreeNodeModel nodeToRemove)
        {
            if (nodeToRemove is FilterTreeNodeModel)
                this._children.Remove(nodeToRemove as FilterTreeNodeModel);
            else if (nodeToRemove is FileTreeNodeModel)
                this._children.Remove(nodeToRemove as FileTreeNodeModel);
        }

        public override void SyncWithLoadValue()
        {
            this.LoadElement();
        }

        public override void SyncWithCurrentValue()
        {
            FilterFiles.Clear();
            foreach(var child in Children)
            {
                if(child is FilterTreeNodeModel)
                {
                    FilterTreeNodeModel filter = child as FilterTreeNodeModel;
                    foreach (var ffitem in filter.ToFilterFileTreeNodeModel) FilterFiles.Add(ffitem);
                }
                else if(child is FileTreeNodeModel)
                {
                    FileTreeNodeModel file = child as FileTreeNodeModel;
                    FilterFiles.Add(new FilterFileTreeNodeModel(string.Empty, file.Path, file.FileName));
                }
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
            var type = TreeNodeCreator.GetType(this.FileName);

            using (StreamWriter wr = new StreamWriter(fullPath))
            {
                XmlSerializer xs = new XmlSerializer(type);
                xs.Serialize(wr, this);
            }
        }

        public override ProjectProperty GetProjectProperty(ProjectProperty.Configure configure)
        {
            return (configure == ProjectProperty.Configure.Debug) ? DebugConfigure
                                                                                            : ReleaseConfigure;
        }
    }
}
