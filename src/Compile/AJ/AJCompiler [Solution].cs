using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Compile.AJ
{
    public partial class AJCompiler
    {
        public void CreateSolution(string absolutePath, string solutionName)
        {
            _absolutePath = absolutePath;
            _solutionName = solutionName;

            Directory.CreateDirectory(Path.GetDirectoryName(_absolutePath));
        }


        public void Save()
        {
            var solutionFullPath = Path.Combine(_absolutePath, _solutionName);

            using StreamWriter wr = new StreamWriter(solutionFullPath);

            XmlSerializer xs = new XmlSerializer(ProjectFullPaths.GetType());
            xs.Serialize(wr, ProjectFullPaths);
        }


        public void Load()
        {
            var solutionFullPath = Path.Combine(_absolutePath, _solutionName);
            List<string> projectRFullPaths = new List<string>();

            using StreamReader sr = new StreamReader(solutionFullPath);
            {
                XmlSerializer xs = new XmlSerializer(typeof(List<string>));
                projectRFullPaths = xs.Deserialize(sr) as List<string>;
            }

            projectRFullPaths.ForEach((projectRFullPath) =>
            {
                var projectPath = Path.GetDirectoryName(projectRFullPath);
                var projectName = Path.GetFileName(projectRFullPath);

                LoadProject(Path.Combine(_absolutePath, projectPath), projectName);
            });
        }


        private string _absolutePath;
        private string _solutionName;
    }
}
