using Parse.MiddleEnd.IR.Datas;
using Parse.Types;
using Parse.Types.ConstantTypes;
using Parse.Types.Operations;
using System;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables
{
    public class StringVariableMiniC : VariableMiniC, IRVar, IString
    {
        public StringVariableMiniC(MiniCTypeInfo typeDatas, 
                                                TokenData nameToken, 
                                                TokenData levelToken, 
                                                TokenData dimensionToken, 
                                                int blockLevel, 
                                                int offset, 
                                                VarProperty varProperty, 
                                                IValue value) : base(typeDatas, nameToken, levelToken, dimensionToken, blockLevel, offset, varProperty, value)
        {
        }

        public override DType TypeName => DType.Unknown;
        public int Size => throw new NotImplementedException();


        public IConstant Equal(IValue operand) => Operation.StringEqual(this, operand);
        public IConstant NotEqual(IValue operand) => Operation.StringNotEqual(this, operand);
    }
}
