using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.LLVM
{
    public class LLVMBuildOption
    {
        public LLVMBuildOption(bool noComment)
        {
            NoComment = noComment;
        }

        public bool NoComment { get; } = false;
    }
}
