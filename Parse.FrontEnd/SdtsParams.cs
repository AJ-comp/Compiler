using Parse.Extensions;
using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Parse.FrontEnd
{
    public class SdtsParams : ICloneable<SdtsParams>
    {
        public SdtsParams(int blockLevel)
        {
            BlockLevel = blockLevel;
        }

        public SdtsParams(int blockLevel, int offset)
        {
            BlockLevel = blockLevel;
            Offset = offset;
        }

        public int BlockLevel { get; set; }
        public int Offset { get; set; }


        public static SdtsParams Create(Stack<object> stack)
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

            return new SdtsParams(block);
        }

        public virtual SdtsParams Clone()
        {
            return null;
        }
        public virtual SdtsParams CloneForNewBlock()
        {
            return null;
        }
    }
}
