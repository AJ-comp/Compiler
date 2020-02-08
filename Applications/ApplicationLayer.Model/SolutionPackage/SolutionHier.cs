using ApplicationLayer.Common.Interfaces;
using Parse.BackEnd.Target;
using Parse.FrontEnd.Grammars;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Xml.Serialization;

namespace ApplicationLayer.Models.SolutionPackage
{
    [XmlInclude(typeof(DefaultProjectHier))]
    public class SolutionHier : HierarchicalData, ISaveAndChangeTrackable
    {
        [XmlIgnore]
        public static string Extension => "ajn";

        private double originalVersion;
        public double Version { get; set; }

        public Collection<PathInfo> ProjectPaths { get; private set; } = new Collection<PathInfo>();

        [XmlIgnore]
        public ObservableCollection<ProjectHier> Projects { get; } = new ObservableCollection<ProjectHier>();

        [XmlIgnore]
        public bool IsChanged
        {
            get
            {
                if (originalVersion != Version) return true;

                if (ProjectPaths.Count != Projects.Count) return true;
                foreach(var project in Projects)
                {
                    var path = new PathInfo(project.AutoPath, project.IsAbsolutePath);
                    if (ProjectPaths.Contains(path) == false) return true;
                }

                return false;
            }
        }

        public SolutionHier()
        {
            this.Projects.CollectionChanged += Projects_CollectionChanged;
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
            SolutionHier result = new SolutionHier();
            string solutionNameWithExtension = Path.GetFileNameWithoutExtension(solutionName);

            result.CurOPath = solutionPath;
            result.FullName = solutionName;
            result.Version = 1.0;

            ProjectGenerator projectGenerator = ProjectGenerator.CreateProjectGenerator(grammar);
            if (projectGenerator == null) return result;

            result.Projects.Add(projectGenerator.CreateDefaultProject(solutionNameWithExtension, false, solutionNameWithExtension, target, result));

            result.Commit();

            return result;
        }

        public void RollBack()
        {
            Version = originalVersion;
        }

        public void Commit()
        {
            originalVersion = Version;

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
    }


    public class PathInfo
    {
        public string Path { get; set; }
        public bool IsAbsolute { get; set; }

        public PathInfo() { }

        public PathInfo(string path, bool isAbsolute)
        {
            Path = path;
            IsAbsolute = isAbsolute;
        }

        public override bool Equals(object obj)
        {
            var info = obj as PathInfo;
            return info != null &&
                   Path == info.Path;
        }

        public override int GetHashCode()
        {
            return 467214278 + EqualityComparer<string>.Default.GetHashCode(Path);
        }

        public override string ToString() => string.Format("[{0},{1}]", this.Path, this.IsAbsolute);
    }
}
