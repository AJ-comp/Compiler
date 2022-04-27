using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using Parse.FrontEnd.Ast;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public class ParamListNode : AJNode, IHasVarList
    {
        public IEnumerable<VariableAJ> VarList => _paramNodes;

        public ParamListNode(AstSymbol node) : base(node)
        {
        }



        // format summary [Can induce epsilon]
        // [0:n] : DeclareVarNode
        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            _paramNodes.Clear();

            var classDefNode = GetParent(typeof(ClassDefNode)) as ClassDefNode;
            _classTypeName = classDefNode.Name;

            foreach (var item in Items)
            {
                var paramNode = item.Compile(param) as DeclareVarNode;

                paramNode.Variable.Param = true;
                _paramNodes.Add(paramNode.Variable);
                param.Offset++;
            }

            return this;
        }


        private List<VariableAJ> _paramNodes = new List<VariableAJ>();
        private string _classTypeName;
    }
}
