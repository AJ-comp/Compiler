using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes;
using Parse.Types;
using Parse.Types.ConstantTypes;
using Parse.Types.Operations;
using System;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables
{
    public class StringVariableMiniC : VariableMiniC, IString
    {
        public StringVariableMiniC(MiniCTypeInfo typeDatas, 
                                                TokenData nameToken, 
                                                TokenData levelToken, 
                                                TokenData dimensionToken, 
                                                int blockLevel, 
                                                int offset, 
                                                VarProperty varProperty, 
                                                ExprNode value) : base(typeDatas, nameToken, levelToken, dimensionToken, blockLevel, offset, varProperty, Convert(value))
        {
        }

        public override DType TypeName => DType.Unknown;
        public int Size => throw new NotImplementedException();


        public IConstant Equal(IValue operand) => Operation.StringEqual(this, operand);
        public IConstant NotEqual(IValue operand) => Operation.StringNotEqual(this, operand);


        public static IValue Convert(ExprNode node)
        {
            return null;
        }

        public override IConstant Assign(IValue operand)
        {
            throw new NotImplementedException();
        }

        public override bool CanAssign(IValue operand)
        {
            throw new NotImplementedException();
        }
    }
}
