using Parse.FrontEnd.AJ;
using Parse.FrontEnd.AJ.Data;
using System.Collections.Generic;
using System.IO;

namespace Compile.AJ
{
    public partial class AJCompiler
    {
        /****************************************************************/
        /// <summary>
        /// <para>Create a project.</para>
        /// <para>프로젝트를 생성합니다.</para>
        /// </summary>
        /// <param name="projectName"></param>
        /****************************************************************/
        public void CreateProject(string projectName)
        {
            // if already there is a same type it isn't added because collection type is hashset.
            var project = new AJProject(projectName, projectName);
            project.Save();

            _assemblyDic.Add(projectName, project);
        }


        /****************************************************************/
        /// <summary>
        /// <para>Create a project.</para>
        /// <para>프로젝트를 생성합니다.</para>
        /// </summary>
        /// <param name="solutionPath"></param>
        /// <param name="projectName"></param>
        /****************************************************************/
        public void CreateProject(string solutionPath, string projectName)
        {
            // if already there is a same type it isn't added because collection type is hashset.
            var projectPath = Path.Combine(solutionPath, projectName);
            var project = new AJProject(projectName, projectPath);
            project.Save();

            _assemblyDic.Add(projectName, project);
        }


        /****************************************************************/
        /// <summary>
        /// <para>Load a project.</para>
        /// <para>프로젝트를 로드합니다.</para>
        /// </summary>
        /// <param name="projectAbsolutePath"></param>
        /// <param name="projectName"></param>
        /****************************************************************/
        public void LoadProject(string projectAbsolutePath, string projectName)
        {
            var project = AJProject.Load(projectName, projectAbsolutePath);

            _assemblyDic.Add(projectName, project);
        }

        /****************************************************************/
        /// <summary>
        /// <para>Unload the project from the solution.</para>
        /// <para>솔루션에서 프로젝트를 언로드합니다.</para>
        /// </summary>
        /// <param name="projectName"></param>
        /****************************************************************/
        public void UnloadProject(string projectName) => _assemblyDic.Remove(projectName);



        /****************************************************************/
        /// <summary>
        /// <para>Add the existing file in the project path to the project.</para>
        /// <para>프로젝트 경로에서 이미 존재하는 파일을 프로젝트에 추가합니다.<br/>
        /// 만약 프로젝트나 파일이 존재하지 않으면 예외가 발생합니다.</para>
        /// </summary>
        /// <param name="projectName">The project name to add the file</param>
        /// <param name="fileRelativePath">The file relative path to add</param>
        /****************************************************************/
        public void AddExistFileToProject(string projectName, string fileRelativePath)
            => _assemblyDic[projectName].AddExistingFile(fileRelativePath);


        /****************************************************************/
        /// <summary>
        /// <para>Add new file to the project. The new file is created only in the project path. </para>
        /// <para>새 파일을 프로젝트에 추가합니다. 새 파일은 프로젝트 경로의 안에서 생성됩니다.</para>
        /// </summary>
        /// <param name="projectName">The project name to add new file</param>
        /// <param name="fileRelativePath">The file relative path to add</param>
        /****************************************************************/
        public void AddNewFileToProject(string projectName, string fileRelativePath)
            => _assemblyDic[projectName].AddNewFile(fileRelativePath);


        /****************************************************************/
        /// <summary>
        /// <para>Remove file from the project.</para>
        /// <para>프로젝트에서 파일을 제거합니다.</para>
        /// </summary>
        /// <param name="projectName">The project name to remove the file</param>
        /// <param name="toRemoveFileRelativePath">The file relative path to remove</param>
        /// <param name="bDelFile">whether delete the file</param>
        /****************************************************************/
        public void RemoveFileFromProject(string projectName, string toRemoveFileRelativePath, bool bDelFile = false)
            => _assemblyDic[projectName].RemoveFile(toRemoveFileRelativePath, bDelFile);


        /****************************************************************/
        /// <summary>
        /// <para>Change the project name.</para>
        /// <para>프로젝트 이름을 바꿉니다.</para>
        /// </summary>
        /// <param name="originalName">The original project name</param>
        /// <param name="toChangeName">The project name to change</param>
        /****************************************************************/
        public void ChangeProjectName(string originalName, string toChangeName)
        {
            _assemblyDic[originalName].ChangeName(toChangeName);

            _assemblyDic.Add(toChangeName, _assemblyDic[originalName]);
            _assemblyDic.Remove(originalName);
        }


        private Dictionary<string, TotalData> _docTable = new Dictionary<string, TotalData>();
        private Dictionary<string, AJProject> _assemblyDic = new Dictionary<string, AJProject>();


        public IEnumerable<string> ProjectFullPaths
        {
            get
            {
                List<string> result = new List<string>();

                foreach (var project in _assemblyDic)
                    result.Add(project.Value.AbsoluteFullPath);

                return result;
            }
        }
    }
}
