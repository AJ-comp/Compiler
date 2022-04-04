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

        public IntegerLiteralNode(AstSymbol node) : base(node)
        {
        }

        public IntegerLiteralNode(int value) : base(null)
        {
            Type = new AJPreDefType(AJDataType.Int);
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

                if (byte.MinValue <= value && value <= byte.MaxValue) AllocType(AJDataType.Bool, false);
                else if (sbyte.MinValue <= value && value <= sbyte.MaxValue) AllocType(AJDataType.Bool, true);
                else if (short.MinValue <= value && value <= short.MaxValue) AllocType(AJDataType.Short, true);
                else if (ushort.MinValue <= value && value <= ushort.MaxValue) AllocType(AJDataType.Short, false);
                else if (int.MinValue <= value && value <= int.MaxValue) AllocType(AJDataType.Int, true);
                else if (uint.MinValue <= value && value <= uint.MaxValue) AllocType(AJDataType.Short, false);
                else AllocType(AJDataType.Double, true);

                Value = value;
                ValueState = State.Fixed;
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

            StdType type = (Type.DataType == AJDataType.Byte)
                                ? StdType.Char
                                : (Type.DataType == AJDataType.Short)
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


        protected void AllocType(AJDataType dataType, bool signed)
        {
            var type = new AJPreDefType(dataType);
            type.Signed = signed;
            Type = type;
        }
    }
}
