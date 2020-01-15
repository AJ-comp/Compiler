namespace ApplicationLayer.Models.SolutionPackage
{
    public class ProjectProperty : HirStruct
    {
        public enum Configure { Debug, Release }

        public Configure Mode { get; set; }
        public string Target { get; set; }
        public int OptimizeLevel { get; set; }

//        public ObservableCollection<>
    }
}
