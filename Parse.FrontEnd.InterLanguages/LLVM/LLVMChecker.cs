namespace Parse.FrontEnd.InterLanguages.LLVM
{
    class LLVMChecker
    {
        public static bool IsGreater(DataType from, DataType to)
        {
            var fromAlign = LLVMConverter.ToAlign(from);
            var toAlign = LLVMConverter.ToAlign(to);

            return (fromAlign > toAlign) ? true : false;
        }
    }
}
