using ApplicationLayer.Common;
using ApplicationLayer.Models;
using ApplicationLayer.Models.SolutionPackage;
using ApplicationLayer.ViewModels.DocumentTypeViewModels;
using ApplicationLayer.ViewModels.Messages;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.Generic;
using System.IO;
using ViewResources = ApplicationLayer.Define.Properties.WindowViewResources;

namespace ApplicationLayer.ViewModels
{
    public class CommandLogic
    {
        public static void BuildProject(string solutionBinFolderPath, ProjectTreeNodeModel projectNode)
        {
            string buildMessage = string.Format(ViewResources.StartBuild_2,
                                                        projectNode.DisplayName,
                                                        projectNode.MCPUType);

            Messenger.Default.Send(new AddBuildMessage("----- " + buildMessage + " -----"));

            // create bc, s by projects
            foreach (var file in projectNode.AllFileNodes)
            {
                if (file is SourceFileTreeNodeModel == false) continue;

                var cFile = file as SourceFileTreeNodeModel;

                if (cFile.Ast?.ErrNodes.Count == 0)
                {
                    //                    File.WriteAllText(cFile.FullPath, cFile.);

                    // create bitcode
                    var bitCodeFile = FileHelper.ConvertFileName(cFile.FileName, ".bc");
                    var bitCodeFullPath = Path.Combine(solutionBinFolderPath, bitCodeFile);
                    string bitCodeMessage = string.Format(ViewResources.GenerateFile_1, bitCodeFile);
                    Builder.CreateBitCode(cFile.Ast, bitCodeFullPath);
                    Messenger.Default.Send(new AddBuildMessage(bitCodeMessage));

                    // create target code
                    var targetCodeFile = FileHelper.ConvertTargetFileName(cFile.FileName);
                    var targetCodeFullPath = Path.Combine(solutionBinFolderPath, targetCodeFile);
                    string targetCodeMessage = string.Format(ViewResources.GenerateFile_1, targetCodeFile);
                    Builder.CreateAssem(bitCodeFullPath, targetCodeFullPath, projectNode.MCPUType);
                    Messenger.Default.Send(new AddBuildMessage(targetCodeMessage));
                }
            }
        }


        /// ********************************************************
        /// <summary>
        /// This function builds all projects
        /// </summary>
        /// <param name="solution"></param>
        /// <returns>referenced file infos on project</returns>
        /// ********************************************************
        public static IEnumerable<FileReferenceInfo> BuildAllProjects(SolutionTreeNodeModel solution)
        {
            List<FileReferenceInfo> result = new List<FileReferenceInfo>();
            foreach (var project in solution.Children)
            {
                var cProject = project as ProjectTreeNodeModel;

                BuildProject(solution.BinFolderPath, cProject);
                result.AddRange(cProject.FileReferenceInfos);
            }

            return result;
        }


        /// ********************************************************
        /// <summary>
        /// This function saves all files
        /// </summary>
        /// <param name="allDocuments"></param>
        /// ********************************************************
        public static void SaveAllFiles(IEnumerable<DocumentViewModel> allDocuments)
        {
            Messenger.Default.Send(new AddBuildMessage("----- Save all files -----"));
            foreach (var document in allDocuments)
            {
                document.Changed = false;

                if (document is EditorTypeViewModel)
                {
                    var editorDoc = document as EditorTypeViewModel;
                    editorDoc.SyncWithFile();
                    Messenger.Default.Send(new AddBuildMessage(string.Format("{0} file saved", editorDoc.FullPath)));
                }
            }
            Messenger.Default.Send(new AddBuildMessage("===== Save Completed ====="));
        }


        /// ********************************************************
        /// <summary>
        /// This function creates bootstrap and linker script
        /// </summary>
        /// <param name="solutionBinFolderPath"></param>
        /// <param name="bootstrapName"></param>
        /// <param name="linkerScriptName"></param>
        /// ********************************************************
        public static void CreateStartingFile(string solutionBinFolderPath, string bootstrapName, string linkerScriptName)
        {
            // create bootstrap
            BootsTrapGenerator.CreateVectorTable(solutionBinFolderPath, bootstrapName);

            // create linker script
            Builder.CreateLinkerScript(solutionBinFolderPath, linkerScriptName);
        }


        /// ********************************************************
        /// <summary>
        /// This function creates makefile sections. this can be used to create makefile.
        /// </summary>
        /// <param name="bootstrapName"></param>
        /// <param name="linkerScriptName"></param>
        /// <param name="binFileName"></param>
        /// <param name="fileReferenceInfos"></param>
        /// <returns>makefile section list</returns>
        /// ********************************************************
        public static IEnumerable<MakeFileSectionStruct> CreateAllMakeFileSection(string bootstrapName,
                                                                                                                        string linkerScriptName,
                                                                                                                        string binFileName,
                                                                                                                        IEnumerable<FileReferenceInfo> fileReferenceInfos)
        {
            List<MakeFileSectionStruct> allMakeFileSnippets = new List<MakeFileSectionStruct>();
            List<string> allObjectFiles = new List<string>();

            allMakeFileSnippets.Add(MakeFileBuilder.CreateCleanSnippet());

            // create makefile object snippet from file reference information
            foreach (var fileRefInfo in fileReferenceInfos)
            {
                Messenger.Default.Send(new AddBuildMessage(string.Format(ViewResources.GenerateFile_1, "makefile")));
                allMakeFileSnippets.Add(MakeFileBuilder.CreateObjectSnippet(string.Empty,
                                                                                                            fileRefInfo.StandardFile,
                                                                                                            fileRefInfo.ReferenceFile));

                // gathering all created object files
                allObjectFiles.Add(FileHelper.ConvertObjectFileName(fileRefInfo.StandardFile));
            }

            // add a bootstrap
            Messenger.Default.Send(new AddBuildMessage(string.Format(ViewResources.GenerateFile_1, "vector table")));
            allMakeFileSnippets.Add(MakeFileBuilder.CreateObjectSnippet(string.Empty, bootstrapName));

            // gathering all created object files
            allObjectFiles.Add(FileHelper.ConvertObjectFileName(bootstrapName));


            // create makefile bin snippet from a entry point (the file that existing main function) of starting project
            allMakeFileSnippets.Add(MakeFileBuilder.CreateBinSnippet(string.Empty,
                                                                                                    binFileName,
                                                                                                    linkerScriptName,
                                                                                                    allObjectFiles));

            // create makefile
            allMakeFileSnippets.Reverse();

            return allMakeFileSnippets;
        }
    }
}
