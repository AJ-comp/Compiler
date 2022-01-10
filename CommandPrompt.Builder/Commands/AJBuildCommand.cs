using CommandPrompt.Builder.Models;
using CommandPrompt.Builder.Properties;
using AJ.Common.Helpers;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Text;

namespace CommandPrompt.Builder.Commands
{
    public abstract class AJBuildCommand : Command
    {
        protected AJBuildCommand(string name, string description = null) : base(name, description)
        {
        }


        public int Execute(Func<int> action)
        {
            try
            {
                return action.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                return ex.HResult;
            }
        }

        public Solution GetSolution(string solutionPath, string solutionName = "")
        {
            solutionPath = solutionPath.AbsolutePath();

            if (string.IsNullOrEmpty(solutionName))
            {
                var files = Directory.GetFiles(solutionPath, $"*{Solution.Extension}");
                if (files.Length != 1)
                {
                    Console.WriteLine(Resource.ErrorReadSolution);
                    return null;
                }

                solutionName = files[0];
            }

            var solutionFullPath = Path.Combine(solutionPath, solutionName);
            return Solution.Read(solutionFullPath);
        }

        public AJProject GetProject(string projectPath, string projectName = "")
        {
            projectPath = projectPath.AbsolutePath();

            if (string.IsNullOrEmpty(projectName))
            {
                var files = Directory.GetFiles(projectPath, $"*{AJProject.Extension}");
                if (files.Length != 1)
                {
                    Console.WriteLine(Resource.ErrorReadProject);
                    return null;
                }

                projectName = files[0];
            }

            var projectFullPath = Path.Combine(projectPath, projectName);
            return AJProject.Read(projectFullPath);
        }
    }
}
