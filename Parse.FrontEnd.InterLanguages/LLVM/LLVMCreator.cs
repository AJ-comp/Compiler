using Parse.FrontEnd.InterLanguages.Datas;
using Parse.FrontEnd.InterLanguages.LLVM.Models;

namespace Parse.FrontEnd.InterLanguages.LLVM
{
    // factory rule
    public class LLVMCreator
    {
        public static LocalSSVar CreateLocalVar(int nextOffset, IRVarData varData)
        {
            if (varData.Type == DataType.Double)
                return new LocalDoubleSSVar(nextOffset, varData);
            else
                return new LocalIntSSVar(nextOffset, varData);
        }

        public static LocalSSVar CreateLocalVar(int nextOffset, SSVarData varData)
        {
            if (varData.Type == DataType.Double)
                return new LocalDoubleSSVar(nextOffset, varData);
            else
                return new LocalIntSSVar(nextOffset, varData);
        }
    }
}
