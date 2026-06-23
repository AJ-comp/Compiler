using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes
{
    public partial class EnumDefNode : TypeDefNode
    {
        public EnumDefNode(AstSymbol node) : base(node)
        {
        }

        public override AJDataType DefType => throw new NotImplementedException();
        public override uint Size => throw new NotImplementedException();
        public override IEnumerable<VariableAJ> AllFields
        {
            get
            {
                List<VariableAJ> result = new List<VariableAJ>();

                return result;
            }
        }

        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            base.CompileLogic(param);

            throw new NotImplementedException();
        }
    }
}
