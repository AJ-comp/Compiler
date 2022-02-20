using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using Parse.Types;
using System;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LiteralNodes
{
    public class IntegerLiteralNode : LiteralNode
    {
        public bool Signed { get; }
        public int Value => (int)Result.Value;

        public IntegerLiteralNode(AstSymbol node) : base(node)
        {
        }

        public IntegerLiteralNode(int value) : base(null)
        {
            Result = new ConstantAJ(value);
        }

        public override SdtsNode Compile(CompileParameter param)
        {
            base.Compile(param);

            try
            {
                double value = 0;
                var node = Items[0].Compile(param) as TerminalNode;
                Token = node.Token;

                if (Token.Kind.TokenType == AJGrammar.HexNumber.TokenType)
                    value = System.Convert.ToInt32(Token.Input, 16);
                else if (Token.Kind.TokenType == AJGrammar.BinNumber.TokenType)
                    value = System.Convert.ToInt32(Token.Input, 2);
                else value = System.Convert.ToInt32(Token.Input);

                if (byte.MinValue <= value && value <= byte.MaxValue) Result = new ConstantAJ((byte)value);
                else if (sbyte.MinValue <= value && value <= sbyte.MaxValue) Result = new ConstantAJ((sbyte)value);
                else if (short.MinValue <= value && value <= short.MaxValue) Result = new ConstantAJ((short)value);
                else if (ushort.MinValue <= value && value <= ushort.MaxValue) Result = new ConstantAJ((ushort)value);
                else if (int.MinValue <= value && value <= int.MaxValue) Result = new ConstantAJ((int)value);
                else if (uint.MinValue <= value && value <= uint.MaxValue) Result = new ConstantAJ((uint)value);
                else Result = new ConstantAJ((double)value);
            }
            catch(Exception)
            {

            }
            finally
            {
                if (RootNode.IsBuild) DBContext.Instance.Insert(this);
            }

            return this;
        }

        public override IRExpression To()
        {
            var result = new IRLiteralExpr();

            StdType type = (Result.Type.DataType == AJDataType.Byte)
                                ? StdType.Char
                                : (Result.Type.DataType == AJDataType.Short)
                                ? StdType.Short
                                : StdType.Int;

            result.Type = new TypeInfo(type, 0);
            result.Value = Value;

            return result;
        }

        public override IRExpression To(IRExpression from)
        {
            throw new NotImplementedException();
        }
    }
}
