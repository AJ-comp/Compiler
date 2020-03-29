using Parse.FrontEnd.Grammars;

namespace ApplicationLayer.Models
{
    public class ProjectData
    {
        public enum ProjectTypes { Project, LibraryProject }

        public Grammar Grammar { get; set; }
        public ProjectTypes ProjectType { get; set; }
    }
}
