using ApplicationLayer.Models.SolutionPackage;
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
        public Grammar Language { get; }
        public Target MachineTarget { get; }

        public CreateSolutionMessage(string solutionPath, string solutionName, bool isCreateSolutionFolder,
                                                Grammar language, Target machineTarget)
        {
            if (isCreateSolutionFolder)
                this.SolutionPath = Path.Combine(solutionPath, solutionName);

            this.SolutionName = solutionName;
            this.Language = language;
            this.MachineTarget = machineTarget;
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
        public string ProjectPath { get; }
        public string ProjectName { get; }
        public Grammar Language { get; }
        public Target MachineTarget { get; }

        public string ProjectFullPath => Path.Combine(this.ProjectPath, this.ProjectName);

        public AddProjectMessage(string projectPath, string projectName, Grammar language, Target machineTarget)
        {
            this.ProjectPath = projectPath;
            this.ProjectName = projectName;
            this.Language = language;
            this.MachineTarget = machineTarget;
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
