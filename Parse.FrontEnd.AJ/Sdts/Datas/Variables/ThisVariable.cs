using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using System.Diagnostics;

namespace Parse.FrontEnd.AJ.Sdts.Datas.Variables
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class ThisVariable
    {
        public ThisVariable(ClassDefNode classData, int blockLevel, int offset)
        {
            _classData = classData;
            new VariableAJ(Access.Private, AJTypeInfo.CreateThisType(classData.Type), null, null, blockLevel, offset);
        }


        private ClassDefNode _classData;

        private string GetDebuggerDisplay()
            => $"{AJGrammar.Const} {_classData.Name} {AJGrammar.This}";
    }
}
