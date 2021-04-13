using Parse.FrontEnd.AJ.Sdts.AstNodes;
using Parse.FrontEnd.AJ.Sdts.Expressions;
using Parse.MiddleEnd.IR.LLVM;
using Parse.MiddleEnd.IR.LLVM.Expressions;

namespace Compile
{
    public class IRExpressionGenerator
    {
        public static string ToLLVMCode(AJNode rootNode)
        {
            string result = string.Empty;
            if (rootNode.IsNeedWhileIRGeneration == false) return result;

            foreach (var node in rootNode.Items)
            {
                result += ToLLVMCode(node as AJNode);
            }

            return result;
        }


        /// ******************************************************************/
        /// <summary>
        /// LLVM IR 생성을 위한 LLVMExpression을 생성합니다.
        /// 파라메터 expression이 루트 Expression이 아닐 경우 null을 반환합니다.
        /// </summary>
        /// <param name="expression">LLVMExpression 생성을 위한 FianlExpression의 루트</param>
        /// <returns></returns>
        /// ******************************************************************/
        public static LLVMExpression GenerateLLVMExpression(AJExpression expression)
        {
            var ssaTable = new LLVMSSATable();

            LLVMRootExpression result = new LLVMRootExpression();

            if (!(expression is ProgramExpression)) return null;
            var cExpression = expression as ProgramExpression;

            foreach (var namespaceExpression in cExpression.Namespaces)
            {
                foreach (var classData in namespaceExpression.Classes)
                {
                    result.FirstLayers.Add(new LLVMClassExpression(classData.ToIRData(), ssaTable));
                }
            }

            return result;
        }
    }
}
