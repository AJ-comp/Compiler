using Parse.Extensions;
using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Parse.FrontEnd
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class CompileParameter : ICloneable<CompileParameter>
    {
        public ISymbolData ParentData { get; set; }
        public SdtsNode RootNode { get; set; }
        public int BlockLevel { get; set; } = 0;
        public int Offset { get; set; } = 0;
        public bool Build { get; set; }

        public Dictionary<string, SdtsNode> ReferenceFiles { get; } = new Dictionary<string, SdtsNode>();


        public CompileParameter()
        {
        }

        public CompileParameter(int blockLevel, int offset, bool build = false)
        {
            BlockLevel = blockLevel;
            Offset = offset;
            Build = build;
        }

        public static CompileParameter Create(Stack<object> stack)
        {
            Stack<object> reverseStack = stack.Reverse();

            int block = 0;
            while(reverseStack.Count > 0)
            {
                var data = reverseStack.Pop();
                if ((data is Terminal) == false) continue;

                string[] blockUpString = { "(", "{" };
                string[] blockDownString = { ")", "}" };
                if (blockUpString.Contains((data as Terminal).Value)) block++;
                else if (blockDownString.Contains((data as Terminal).Value)) block--;
            }

            var result = new CompileParameter();
            result.BlockLevel = block;

            return result;
        }

        public virtual CompileParameter Clone()
        {
            var result = new CompileParameter();

            result.RootNode = RootNode;
            result.BlockLevel = BlockLevel;
            result.Offset = Offset;
            result.Build = Build;

            return result;
        }

        public virtual CompileParameter CloneForNewBlock(int offset = 0)
        {
            var result = Clone();
            result.BlockLevel++;
            result.Offset = offset;

            return result;
        }

        private string GetDebuggerDisplay() => $"[Block: {BlockLevel}, Offset: {Offset}, Build: {Build}]";
    }
}
