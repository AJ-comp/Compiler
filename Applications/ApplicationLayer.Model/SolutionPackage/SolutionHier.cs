using ApplicationLayer.Common.Interfaces;
using Parse.BackEnd.Target;
using Parse.FrontEnd.Grammars;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Xml.Serialization;

namespace ApplicationLayer.Models.SolutionPackage
{
    [XmlInclude(typeof(DefaultProjectHier))]
    [XmlRoot ("Solution")]
    public class SolutionHier : HierarchicalData, ISaveAndChangeTrackable
    {
        [XmlIgnore]
        public static string Extension => "ajn";

        [XmlIgnore]
        public double CurrentVersion { get; set; }
        [XmlElement("Version")]
        public double OriginalVersion { get; set; }

        public Collection<PathInfo> ProjectPaths { get; private set; } = new Collection<PathInfo>();

        [XmlIgnore]
        public ObservableCollection<ProjectHier> Projects { get; } = new ObservableCollection<ProjectHier>();

        [XmlIgnore]
        public bool IsChanged
        {
            get
            {
                if (OriginalVersion != CurrentVersion) return true;

                if (ProjectPaths.Count != Projects.Count) return true;
                foreach(var project in Projects)
                {
                    var path = new PathInfo(project.AutoPath, project.IsAbsolutePath);
                    if (ProjectPaths.Contains(path) == false) return true;
                }

                return false;
            }
        }

        public override string DisplayName { get => NameWithoutExtension; }

        private SolutionHier() : this(string.Empty, string.Empty)
        { }

        public SolutionHier(string curOpath, string fullName) : base(curOpath, fullName)
        {
            this.Projects.CollectionChanged += Projects_CollectionChanged;

            this.ToChangeDisplayName = this.DisplayName;
        }

        private void Projects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for(int i=0; i<e.NewItems?.Count; i++)
            {
                ProjectHier child = e.NewItems[i] as ProjectHier;
                child.Parent = this;
            }
        }

        private void ErrorProjects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < e.NewItems?.Count; i++)
            {
                ErrorProjectHier child = e.NewItems[i] as ErrorProjectHier;
                child.Parent = this;
            }
        }

        /// <summary>
        /// This function removes the child after find child type.
        /// </summary>
        /// <param name="child">child to remove</param>
        public void RemoveChild(HierarchicalData child)
        {
            if (child is ProjectHier) this.Projects.Remove(child as ProjectHier);
        }

        public static SolutionHier Create(string solutionPath, string solutionName, Grammar grammar, Target target)
        {
            SolutionHier result = new SolutionHier(solutionPath, solutionName);
            string solutionNameWithExtension = Path.GetFileNameWithoutExtension(solutionName);
            result.CurrentVersion = 1.0;

            ProjectGenerator projectGenerator = ProjectGenerator.CreateProjectGenerator(grammar);
            if (projectGenerator == null) return result;

            result.Projects.Add(projectGenerator.CreateDefaultProject(solutionNameWithExtension, false, solutionNameWithExtension, target, result));
            result.Commit();

            return result;
        }

        public void RollBack() => CurrentVersion = OriginalVersion;

        public void Commit()
        {
            OriginalVersion = CurrentVersion;

            ProjectPaths.Clear();
            foreach (var project in Projects)
                ProjectPaths.Add(new PathInfo(project.AutoPath, project.IsAbsolutePath));
        }

        public override void Save()
        {
            using (StreamWriter wr = new StreamWriter(this.FullPath))
            {
                XmlSerializer xs = new XmlSerializer(typeof(SolutionHier));
                xs.Serialize(wr, this);
            }
        }

        public override void ChangeDisplayName()
        {
            string extension = Path.GetExtension(this.FullName);

            string destFullPath = Path.Combine(this.BaseOPath, this.ToChangeDisplayName + extension);
            File.Move(this.FullPath, destFullPath);

            this.FullName = this.ToChangeDisplayName + extension;
        }

        public override void CancelChangeDisplayName() => this.ToChangeDisplayName = this.NameWithoutExtension;


        public static SolutionHier Load(string curOpath, string fullName, string fullPath)
        {
            SolutionHier result = new SolutionHier(curOpath, fullName);

            using (StreamReader sr = new StreamReader(fullPath))
            {
                XmlSerializer xs = new XmlSerializer(typeof(SolutionHier));
                result = xs.Deserialize(sr) as SolutionHier;
                result.CurOPath = curOpath;
                result.FullName = fullName;
                result.ToChangeDisplayName = result.DisplayName;

                result.RollBack();
            }

            return result;
        }
    }
}
