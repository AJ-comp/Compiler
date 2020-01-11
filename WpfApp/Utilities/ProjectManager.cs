using Parse.FrontEnd.Grammars;
using System.IO;
using WpfApp.Utilities.GeneratorPackages.ProjectGenerators;
using WpfApp.Utilities.GeneratorPackages.ProjectLoaders;
using WpfApp.Utilities.GeneratorPackages.ProjectStructs;

namespace WpfApp.Utilities
{
    public class ProjectManager
    {
        public ProjectLoader ProjectLoader { get; }
        public ProjectGenerator ProjectGenerator { get; }
        public ProjectStruct ProjectStruct { get; }

        public ProjectManager(string extension)
        {
            // Factory technique
            if (extension == "mc")
            {
                this.ProjectGenerator = new MiniCGenerator();
                this.ProjectLoader = new MiniCLoader();
                this.ProjectStruct = new MiniCProjectStruct();
            }
            else if(extension == "aj")
            {
                // create Generator, Loader related to AJ
            }
        }

        public void LoadProject(string extension)
        {

        }

        public void CreateProject(ProjectStruct projectStruct, bool bOverWrite)
        {
            if(bOverWrite)
            {
                
            }
        }
    }


    public class FileManager
    {
        public string FilePath { get; }
        public string FileName { get; }

        public string FileData { get; }
    }
}
