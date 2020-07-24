using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Datas.Types;
using Parse.MiddleEnd.IR.Datas.ValueDatas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.Datas
{
    public class MiniCValueData : ValueData
    {
        public override bool IsZero => throw new NotImplementedException();
        public override DType TypeName => throw new NotImplementedException();
        public override object Value => throw new NotImplementedException();
        public override bool Signed => throw new NotImplementedException();
        public override bool IsNan => throw new NotImplementedException();

        public override IRValue Add(IRValue t)
        {
            throw new NotImplementedException();
        }

        public override IRValue Div(IRValue t)
        {
            throw new NotImplementedException();
        }

        public override IRValue Mod(IRValue t)
        {
            throw new NotImplementedException();
        }

        public override IRValue Mul(IRValue t)
        {
            throw new NotImplementedException();
        }

        public override IRValue Sub(IRValue t)
        {
            throw new NotImplementedException();
        }
    }
}
