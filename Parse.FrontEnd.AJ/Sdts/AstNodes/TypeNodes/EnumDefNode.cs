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

        public override AJDataType Type => throw new NotImplementedException();
        public override uint Size => throw new NotImplementedException();
        public override string Name => throw new NotImplementedException();
        public override string FullName => throw new NotImplementedException();


        public override SdtsNode Compile(CompileParameter param)
        {
            base.Compile(param);

            throw new NotImplementedException();
        }

        protected override bool IsDuplicated(TokenData tokenToAdd)
        {
            throw new NotImplementedException();
        }
    }
}
