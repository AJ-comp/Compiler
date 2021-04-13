using Compile.Models;
using Parse.BackEnd.Target;
using Parse.FrontEnd;
using Parse.FrontEnd.AJ.Sdts.AstNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.ArithmeticExprNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LiteralNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes;
using Parse.FrontEnd.AJ.Sdts.Expressions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Compile.AJ
{
    public partial class AJCompiler
    {
        /****************************************************************/
        /// <summary>
        /// 프로젝트와 해당 프로젝트에 의존하는 프로젝트들을 빌드합니다
        /// </summary>
        /// <param name="projName">빌드할 프로젝트 명</param>
        /// <returns></returns>
        /****************************************************************/
        public bool BuildProject(string projName)
        {
            foreach (var fileFullPath in _assemblyDic[projName].FileFullPaths)
            {
                var parsingResult = NewParsing(fileFullPath);
                bool result = parsingResult.Success;
                if (result) StartSemanticAnalysis(fileFullPath);

                var doc = _docTable[fileFullPath];
                doc.FinalExpression = CreateFinalExpression(doc.RootNode);
            }

            return true;
        }


        /****************************************************************/
        /// <summary>
        /// This function builds all projects
        /// 모든 프로젝트를 빌드 합니다.
        /// 만일 컴파일 오류가 있다면 예외가 발생합니다.
        /// </summary>
        /// <returns></returns>
        /****************************************************************/
        public bool AllBuild()
        {
            bool result = true;
//            List<FileReferenceInfo> result = new List<FileReferenceInfo>();

            foreach (var assembly in _assemblyDic)
            {
                if (!BuildProject(assembly.Key)) result = false;

                //                result.AddRange(assembly.)
            }

            return result;
        }


        /****************************************************************/
        /// <summary>
        /// 빌드 후 생성된 fullPath에 해당하는 Expression을 가져옵니다.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns>생성된 Expression의 루트</returns>
        /****************************************************************/
        public AJExpression GetFinalExpression(string fullPath)
        {
            try
            {
                return _docTable[fullPath].FinalExpression;
            }
            catch
            {
                return null;
            }
        }

        public void Optimization(SdtsNode root)
        {
            for (int i = 0; i < root.Items.Count; i++)
            {
                var item = root.Items[i];

                if (item is ArithmeticExprNode)
                {
                    var arthNode = (item as ArithmeticExprNode);
                    if (arthNode.IsBothLiteral)
                        root.Items[i] = LiteralNode.CreateLiteralNode(arthNode.Result);
                }
            }
        }


        /****************************************************************/
        /// <summary>
        /// FinalExpression을 생성합니다.
        /// FinalExpression은 최종단계에 만들어지는 Expression 입니다.
        /// FinalExpression을 통해 최적화와 가상코드 추가 프로세스가 진행됩니다.
        /// </summary>
        /// <param name="rootNode">FinalExpression을 만들기 위한 루트 AST</param>
        /// <returns></returns>
        /****************************************************************/
        public ProgramExpression CreateFinalExpression(SdtsNode rootNode)
        {
            ProgramExpression result = new ProgramExpression();

            foreach (var item in rootNode.Items)
            {
                if (item is UsingStNode)
                    result.Usings.Add(new UsingExpression(item as UsingStNode));
                else if (item is NamespaceNode)
                    result.Namespaces.Add(new NamespaceExpression(item as NamespaceNode));
            }

            return result;
        }


        public void GenerateOutput(string outputPath, string binFileName, Target target)
        {
            // create bin folder.
            Directory.CreateDirectory(outputPath);

            // save all opened files and build all project
            var fileReferenceInfos = AllBuild();

            // create bootstrap and linker script
            var bootstrapName = "vector.s";
            var linkerScriptName = "stm32.lds";
            Builder.CreateStartingFile(outputPath, bootstrapName, linkerScriptName, target);

            /*
            // create makefile and execute it
            var allMakeFileSnippets = MakeFileBuilder.CreateAllMakeFileSection(bootstrapName,
                                                                                                              linkerScriptName,
                                                                                                              binFileName,
                                                                                                              fileReferenceInfos);

            MakeFileBuilder.CreateMakeFile(Path.Combine(outputPath, "makefile"), allMakeFileSnippets);
            Builder.ExecuteMakeFile(outputPath);

            Builder.CreateJLinkCommanderScript(outputPath, binFileName, "0x08000000");
            */
        }
    }
}
