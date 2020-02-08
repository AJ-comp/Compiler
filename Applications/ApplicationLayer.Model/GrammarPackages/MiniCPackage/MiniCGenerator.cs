﻿using ApplicationLayer.Models.SolutionPackage;
using Parse.BackEnd.Target;
using System.IO;
using static ApplicationLayer.Models.SolutionPackage.ProjectProperty;

namespace ApplicationLayer.Models.GrammarPackages.MiniCPackage
{
    public class MiniCGenerator : ProjectGenerator
    {
        public override string Extension { get; } = LanguageExtensions.MiniC;

        public override DefaultProjectHier CreateDefaultProject(string projectPath, bool isAbsolutePath, string projectName, Target target, HierarchicalData parent)
        {
            DefaultProjectHier result = this.CreateEmptyProject(projectPath, isAbsolutePath, projectName, target, parent);

            result.ReferenceFolder[0].Items.Add(new ReferenceFileStruct() { CurOPath = "MiniC", FullName = "System.dll" });
            result.ReferenceFolder[0].Items.Add(new ReferenceFileStruct() { CurOPath = "MiniC", FullName = "System.IO.dll" });
            result.ReferenceFolder[0].Items.Add(new ReferenceFileStruct() { CurOPath = "MiniC", FullName = "System.Data.dll" });

            string fileName = string.Format("main.{0}", this.Extension);
            string fileData = "void main()\r\n{\r\n}";

            var fileHier = new DefaultFileHier()
            {
                FullName = fileName,
                Data = fileData
            };
            result.Items.Add(fileHier);
            fileHier.Save();

            result.Commit();

            return result;
        }

        public override DefaultProjectHier CreateEmptyProject(string projectPath, bool isAbsolutePath, string projectName, Target target, HierarchicalData parent)
        {
            DefaultProjectHier result = new DefaultProjectHier
            {
                Parent = parent,

                CurOPath = projectPath,
                FullName = string.Format("{0}.{1}", projectName, this.Extension + "proj"),
                Version = 1.0
            };

            ProjectProperty debugProperty = new ProjectProperty
            {
                Mode = Configure.Debug,
                Target = target.Name,
                OptimizeLevel = 0
            };

            ProjectProperty releaseProperty = new ProjectProperty
            {
                Mode = Configure.Release,
                Target = target.Name,
                OptimizeLevel = 0
            };

            result.Properties.Add(debugProperty);
            result.Properties.Add(releaseProperty);

            //            result.ReferenceFolder

            return result;
        }
    }
}
