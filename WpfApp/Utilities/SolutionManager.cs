using System.Collections.Generic;
using WpfApp.Models;

namespace WpfApp.Utilities
{
    public class SolutionManager
    {
        private static SolutionManager instance;
        public SolutionManager Instance
        {
            get
            {
                if (instance == null) instance = new SolutionManager();

                return instance;
            }
        }

        public SolutionLoader Loader = new SolutionLoader();
        public SolutionGenerator Generator = new SolutionGenerator();

        public SolutionStruct SolutionStruct { get; } = new SolutionStruct();
        public List<ProjectManager> ProjectManagers { get; } = new List<ProjectManager>();
    }
}
