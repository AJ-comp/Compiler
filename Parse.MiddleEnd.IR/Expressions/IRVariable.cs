using Parse.MiddleEnd.IR.Datas;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.Expressions
{
    public class IRVariable : IRExpression
    {
        public IRType Type { get; }
        public string Name { get; }
        public bool IsGlobal { get; set; } = false;
        public int BlockIndex { get; set; }
        public int OffsetIndex { get; set; }
        public int AbsIndexInLocal { get; set; }


        public string LLVMTypeName => Type.LLVMTypeName;


        public IRVariable(IRType type, string name)
        {
            Type = type;
            Name = name;
        }

        /**********************************************************/
        /// <summary>
        /// This function returns a data format as below <br/>
        /// IsGlobal is true: 
        /// <b><i>@{name} = common global {type} {initValue}, align {size} </i></b>
        /// <br/>
        /// IsGlobal is false: 
        /// <b><i>%{index} = alloca {type}, allign {size}</i></b> <br/>
        /// </summary>
        /// <returns></returns>
        /**********************************************************/
        /*
        public override IEnumerable<string> Build()
        {
            List<string> result = new List<string>();

            if (IsGlobal)
            {
                string initValue = "0";
                if (Type.PointerLevel > 0) initValue = "null";
                else if (Type.Type == Types.StdType.Struct) initValue = "zeroinitializer";

                result.Add($"{Name} = common global {Type.LLVMTypeName} {initValue}, align {Type.Size}");
            }
            else
            {
                result.Add($"%{Name} = alloca {Type.LLVMTypeName}, align {Type.Signed}");
            }

            return result;
        }
        */
    }
}
