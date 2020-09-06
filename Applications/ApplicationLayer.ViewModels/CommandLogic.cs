using ApplicationLayer.Common;
using ApplicationLayer.Models.SolutionPackage;
using ApplicationLayer.ViewModels.Messages;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ApplicationLayer.ViewModels
{
    public class CommandLogic
    {
        public static void BuildProject(SolutionTreeNodeModel solution, ProjectTreeNodeModel projectNode)
        {
            // create bc, s by projects
            foreach (var file in projectNode.AllFileNodes)
            {
                if (file is SourceFileTreeNodeModel == false) continue;

                var cFile = file as SourceFileTreeNodeModel;

                if (cFile.Ast?.ErrNodes.Count == 0)
                {
                    var bitCodeFile = FileHelper.ConvertFileName(cFile.FileName, ".bc");
                    var bitCodeFullPath = Path.Combine(solution.BinFolderPath, bitCodeFile);
                    Builder.CreateBitCode(cFile.Ast, bitCodeFullPath);
                    Messenger.Default.Send(new AddBuildMessage(string.Format("generate {0}", bitCodeFile)));

                    var targetCodeFile = FileHelper.ConvertTargetFileName(cFile.FileName);
                    var targetCodeFullPath = Path.Combine(solution.BinFolderPath, targetCodeFile);
                    Builder.CreateAssem(bitCodeFullPath, targetCodeFullPath, projectNode.MCPUType);
                    Messenger.Default.Send(new AddBuildMessage(string.Format("generate {0}", targetCodeFile)));
                }
            }
        }
    }
}
