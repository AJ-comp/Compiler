using ApplicationLayer.Models;
using GalaSoft.MvvmLight.Messaging;
using Parse.BackEnd.Target;
using Parse.FrontEnd.Grammars;
using System.IO;

namespace ApplicationLayer.ViewModels.Messages
{
    /// <summary>
    /// This message informs that target has to create a solution.
    /// </summary>
    public class CreateSolutionMessage : MessageBase
    {
        public string SolutionPath { get; }
        public string SolutionName { get; }
        public ProjectType ProjectType { get; }
        public Target MachineTarget { get; }

        public CreateSolutionMessage(string solutionPath, 
                                                    string solutionName, 
                                                    bool isCreateSolutionFolder,
                                                    ProjectType projectType,
                                                    Target machineTarget)
        {
            if (isCreateSolutionFolder)
                SolutionPath = Path.Combine(solutionPath, solutionName);

            SolutionName = solutionName;
            ProjectType = projectType;
            MachineTarget = machineTarget;
        }
    }

    /// <summary>
    /// This message informs that target has to load a solution.
    /// </summary>
    public class LoadSolutionMessage : MessageBase
    {
        public string SolutionFullPath { get; }

        public string SolutionPath => Path.GetDirectoryName(this.SolutionFullPath);
        public string SolutionName => Path.GetFileName(this.SolutionFullPath);
        public string SolutionNameWithOutExtension => Path.GetFileNameWithoutExtension(this.SolutionFullPath);

        public LoadSolutionMessage(string solutionFullPath)
        {
            this.SolutionFullPath = solutionFullPath;
        }
    }

    /// <summary>
    /// This message informs that target has to add a new project to the solution.
    /// </summary>
    public class AddProjectMessage : MessageBase
    {
        public ProjectData ProjectData { get; }
        public Target MachineTarget { get; }

        public AddProjectMessage(ProjectData projectData, Target machineTarget)
        {
            ProjectData = projectData;
            MachineTarget = machineTarget;
        }
    }

    /// <summary>
    /// This messages informs that target has to load a new project to the solution.
    /// </summary>
    public class LoadProjectMessage : MessageBase
    {
        public string ProjectFullPath { get; }

        public LoadProjectMessage(string projectFullPath)
        {
            ProjectFullPath = projectFullPath;
        }
    }

    /// <summary>
    /// This message informs that the target has to add a changed project or solution files that missed.
    /// </summary>
    public class AddMissedChangedFilesMessage : MessageBase
    {
    }
}
