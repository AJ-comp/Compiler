namespace WpfApp.Models
{
    public class ProjectData
    {
        public enum ProjectTypes { Project, LibraryProject }

        public string ImageSrc { get; set; }
        public string Name { get; set; }
        public ProjectTypes ProjectType { get; set; }
    }
}
