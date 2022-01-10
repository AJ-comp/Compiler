using Parse.FrontEnd.AJ.Data;
using System;
using System.IO;
using System.Linq;

namespace Compile.AJ
{
    public partial class AJCompiler
    {
        /****************************************************************/
        /// <summary>
        /// <para>Copy the file.</para>
        /// <para>파일을 복사합니다.</para>
        /// </summary>
        /// <param name="originalProjectName"></param>
        /// <param name="originalFilePath"></param>
        /// <param name="targetProjectName"></param>
        /// <param name="targetFilePath"></param>
        /****************************************************************/
        public void CopyFile(string originalProjectName, string originalFilePath, string targetProjectName, string targetFilePath)
        {
            var originalFileFullPath = Path.Combine(originalProjectName, originalFilePath);
            var targetFileFullPath = Path.Combine(targetProjectName, targetFilePath);

            File.Copy(originalFileFullPath, targetFileFullPath);

            _assemblyDic[targetProjectName].AddExistingFile(targetFilePath);
        }


        /****************************************************************/
        /// <summary>
        /// <para>Move the file within the same project. </para>
        /// <para>동일한 프로젝트 내에서 파일을 이동시킵니다. </para>
        /// </summary>
        /// <param name="originalProjectName"></param>
        /// <param name="originalFilePath"></param>
        /// <param name="targetProjectName"></param>
        /// <param name="targetFilePath"></param>
        /****************************************************************/
        public void MoveFile(string originalProjectName, string originalFilePath, string targetProjectName, string targetFilePath)
        {
            CopyFile(originalProjectName, originalFilePath, targetProjectName, targetFilePath);
            _assemblyDic[originalProjectName].RemoveFile(originalFilePath, true);
        }


        /****************************************************************/
        /// <summary>
        /// <para>Get the project has the file.</para>
        /// <para>해당 파일이 속한 프로젝트를 가져옵니다.</para>
        /// </summary>
        /// <param name="fileFullPath"></param>
        /// <returns></returns>
        /****************************************************************/
        private AJProject GetProjectHasFile(string fileFullPath)
        {
            AJProject result = null;
            var fileName = Path.GetFileName(fileFullPath);

            foreach (var project in _assemblyDic)
            {
                if (project.Value.InternalFileRFullPaths.Contains(fileName))
                {
                    result = project.Value;
                    break;
                }
            }

            return result;
        }
    }
}
