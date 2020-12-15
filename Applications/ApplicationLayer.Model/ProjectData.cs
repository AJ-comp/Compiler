using ApplicationLayer.Common;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.MiniC;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace ApplicationLayer.Models
{
    public enum ProjectKinds 
    { 
        [Description("Unknown")] Unknown, 
        [Description("Project")] Execute,
        [Description("Library Project")] Library 
    }

    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public class ProjectType
    {
        public Grammar Grammar { get; set; }
        public ProjectKinds ProjectKind { get; set; }

        public ProjectType(Grammar grammar, ProjectKinds projectKind)
        {
            Grammar = grammar;
            ProjectKind = projectKind;
        }


        public string DebuggerDisplay
            => string.Format("Grammar: {0}, ProjectKind: {1}", Grammar.GetType().Name, ProjectKind);
    }


    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public class ProjectData
    {
        public string ProjectPath { get; }
        public string ProjectName { get; }
        public ProjectType ProjectType { get; }

        public string ProjectNameWithExtension => ProjectName + ProjectExtension;
        public string ProjectFullPath => Path.Combine(ProjectPath, ProjectName);
        public string ProjectExtension
        {
            get
            {
                string result = string.Empty;

                if (ProjectType.Grammar is MiniCGrammar)
                    result = string.Format(".{0}project", LanguageExtensions.MiniCSource);

                return result;
            }
        }

        public ProjectData(string projectPath, string projectName, ProjectType projectType)
        {
            ProjectPath = projectPath;
            ProjectName = projectName;
            ProjectType = projectType;
        }

        public ProjectData(string projectPath, string projectName, ProjectKinds projectKind)
        {
            ProjectPath = projectPath;
            ProjectName = projectName;

            ProjectType = new ProjectType(null, projectKind);
        }


        private string DebuggerDisplay
            => string.Format(ProjectFullPath, ", {0}", ProjectType.DebuggerDisplay);
    }
}
