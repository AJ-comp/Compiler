using GalaSoft.MvvmLight.Messaging;
using Parse.BackEnd.Target;
using Parse.FrontEnd.Grammars;
using System.IO;

namespace ApplicationLayer.ViewModels.Messages
{
    public class CreateSolutionMessage : MessageBase
    {
        public string SolutionPath { get; }
        public string SolutionNameWithOutExtension { get; }
        public string Extension { get; } = ".ajn";
        public Grammar Language { get; }
        public Target MachineTarget { get; }

        public string SoltionName => this.SolutionNameWithOutExtension + this.Extension;
        public string SolutionFullPath => Path.Combine(this.SolutionPath, this.SoltionName);

        public CreateSolutionMessage(string solutionPath, string solutionNameWithOutExtension, bool isCreateSolutionFolder,
                                                Grammar language, Target machineTarget)
        {
            if (isCreateSolutionFolder)
                this.SolutionPath = Path.Combine(solutionPath, solutionNameWithOutExtension);

            this.SolutionNameWithOutExtension = solutionNameWithOutExtension;
            this.Language = language;
            this.MachineTarget = machineTarget;
        }
    }

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
}
