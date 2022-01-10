using Parse.FrontEnd.AJ.Sdts.AstNodes;

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
    }
}
