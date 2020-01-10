using GalaSoft.MvvmLight.Messaging;

namespace WpfApp.Messages
{
    public class LoadSolutionMessage : MessageBase
    {
        public string SolutionPath { get; }
        public string SolutionName { get; }

        public LoadSolutionMessage(string solutionPath, string solutionName)
        {
            SolutionPath = solutionPath;
            SolutionName = solutionName;
        }
    }
}
