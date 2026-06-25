using Parse.BackEnd.Target;
using Parse.FrontEnd;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Binary;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LiteralNodes;
using System.IO;

namespace Compile.AJ
{
    public partial class AJCompiler
    {
        /****************************************************************/
        /// <summary>
        /// <para>Build the project and related to projects.</para>
        /// <para>프로젝트와 해당 프로젝트에 의존하는 프로젝트들을 빌드합니다.</para>
        /// </summary>
        /// <param name="projName">The project name to build</param>
        /// <returns></returns>
        /****************************************************************/
        public bool BuildProject(string projName)
        {
            foreach (var fileFullPath in _assemblyDic[projName].SourceFileAFullPaths)
            {
                var parsingResult = NewParsing(fileFullPath);
                bool result = parsingResult.Success;

                var compileParam = new CompileParameter();
                if (result) StartSemanticAnalysis(compileParam, true);

                var doc = _docTable[fileFullPath];
            }

            return true;
        }


        public CompileResult Compile(CompileParameter parameter)
        {
            var parsingResult = NewParsing(parameter.FileFullPath);
            if (parsingResult.Success) StartSemanticAnalysis(parameter, true);

            return new CompileResult(parameter.FileFullPath, parsingResult);
        }


        /****************************************************************/
        /// <summary>
        /// <para>Build all projects.</para>
        /// <para>모든 프로젝트를 빌드 합니다.</para>
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


        public void Optimization(SdtsNode root)
        {
            for (int i = 0; i < root.Items.Count; i++)
            {
                var item = root.Items[i];

                if (item is ArithmeticNode)
                {
                    var arthNode = item as ArithmeticNode;
                    if (arthNode.IsBothLiteral)
                        root.Items[i] = LiteralNode.CreateLiteralNode(arthNode);
                }
            }
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
