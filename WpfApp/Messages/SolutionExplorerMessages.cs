using GalaSoft.MvvmLight.Messaging;
using Parse.BackEnd.Target;
using Parse.FrontEnd.Grammars;

namespace WpfApp.Messages
{
    public class CreateSolutionMessage : MessageBase
    {
        public string SolutionPath { get; }
        public string SolutionName { get; }
        public bool IsCreateSolutionFolder { get; }
        public Grammar Language { get; }
        public Target MachineTarget { get; }

        public CreateSolutionMessage(string solutionPath, string solutionName, bool isCreateSolutionFolder,
                                                Grammar language, Target machineTarget)
        {
            this.SolutionPath = solutionPath;
            this.SolutionName = solutionName;
            this.IsCreateSolutionFolder = isCreateSolutionFolder;
            this.Language = language;
            this.MachineTarget = machineTarget;
        }
    }

    public class LoadSolutionMessage : MessageBase
    {
        public string SolutionPath { get; }
        public string SolutionName { get; }

        public LoadSolutionMessage(string solutionPath, string solutionName)
        {
            this.SolutionPath = solutionPath;
            this.SolutionName = solutionName;
        }
    }
}
