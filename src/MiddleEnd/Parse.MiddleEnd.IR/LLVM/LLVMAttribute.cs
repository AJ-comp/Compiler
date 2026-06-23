using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Parse.MiddleEnd.IR.LLVM
{
    public class LLVMAttribute
    {
//        [Description ("norecurse nounwind readnone")]
        public bool NounwindReadnone { get; set; }

//        [Description("\"stack-protector-buffer-size\"")]
        public int StackProtectorBufferSize { get; set; } = 0;


        public LLVMAttribute(int index)
        {
            _index = index;
        }

        public override string ToString()
        {
            StringCollection attributes = new StringCollection();

            if (NounwindReadnone) 
                attributes.Add("norecurse nounwind readnone");
            if (StackProtectorBufferSize > 0) 
                attributes.Add("\"stack-protector-buffer-size\"" + "=" + "\"" + StackProtectorBufferSize + "\"");

            string result = string.Empty;
            foreach (var attribute in attributes)
                result += attribute + Environment.NewLine;

            if (result.Length > 0)
            {
                result = "{" + Environment.NewLine + result + "}";
                result = "attributes #" + _index + " =" + Environment.NewLine + result;
            }

            return result;
        }

        private int _index;
    }
}
