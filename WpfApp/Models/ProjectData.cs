using Parse.FrontEnd.Grammars;

namespace WpfApp.Models
{
    public class ProjectData
    {
        public enum ProjectTypes { Project, LibraryProject }

        public string ImageSrc { get; set; }
        public Grammar Grammar { get; set; }
        public ProjectTypes ProjectType { get; set; }
    }
}
