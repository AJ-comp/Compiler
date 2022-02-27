using ApplicationLayer.Models.SolutionPackage;
using ApplicationLayer.ViewModels.DocumentTypeViewModels;
using ApplicationLayer.ViewModels.Messages;
using Compile;
using Compile.AJ;
using Compile.Models;
using GalaSoft.MvvmLight.Messaging;
using Parse.BackEnd.Target;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ViewResources = ApplicationLayer.Define.Properties.WindowViewResources;

namespace ApplicationLayer.ViewModels
{
    public class CommandLogic
    {
        public static void BuildProject(string solutionBinFolderPath, ProjectTreeNodeModel projectNode, AJCompiler compiler)
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

                if (cFile.Ast?.AllAlarmNodes.Count() == 0)
                {
                    //                    File.WriteAllText(cFile.FullPath, cFile.);

                    // create bitcode
                    var bitCodeFile = FileHelper.ConvertFileName(cFile.FileName, ".bc");
                    var bitCodeFullPath = Path.Combine(solutionBinFolderPath, bitCodeFile);
                    string bitCodeMessage = string.Format(ViewResources.GenerateFile_1, bitCodeFile);
                    compiler.AllBuild();
//                    Builder.CreateBitCode(finalExpression, bitCodeFullPath);
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
    }
}
