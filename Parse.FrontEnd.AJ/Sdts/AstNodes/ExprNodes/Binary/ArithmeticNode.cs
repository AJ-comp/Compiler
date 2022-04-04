using AJ.Common.Helpers;
using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using Parse.Types;
using System;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Binary
{
    public class ArithmeticNode : BinaryExprNode
    {
        public IRArithmeticOperation Operation { get; }

        public ArithmeticNode(AstSymbol node, IRArithmeticOperation operation) : base(node)
        {
            Operation = operation;
        }

        public override SdtsNode Compile(CompileParameter param)
        {
            base.Compile(param);

            if (!IsCanParsing) return this;

            Value = null;
            ValueState = State.NotFixed;

            if (Operation == IRArithmeticOperation.Add) Add(LeftNode, RightNode);
            else if (Operation == IRArithmeticOperation.Sub) Sub(LeftNode, RightNode);
            else if (Operation == IRArithmeticOperation.Mul) Mul(LeftNode, RightNode);
            else if (Operation == IRArithmeticOperation.Div) Div(LeftNode, RightNode);
            else if (Operation == IRArithmeticOperation.Mod) Mod(LeftNode, RightNode);

            if (Type == null) Alarms.Add(AJAlarmFactory.CreateMCL0023(this, Operation.ToDescription()));
            if (RootNode.IsBuild) DBContext.Instance.Insert(this);

            return this;
        }

        public override IRExpression To()
        {
            var result = new IRBinaryExpr();

            if (Operation == IRArithmeticOperation.Add) result.Operation = IRBinaryOperation.Add;
            else if (Operation == IRArithmeticOperation.Sub) result.Operation = IRBinaryOperation.Sub;
            else if (Operation == IRArithmeticOperation.Mul) result.Operation = IRBinaryOperation.Mul;
            else if (Operation == IRArithmeticOperation.Div) result.Operation = IRBinaryOperation.Div;
            else if (Operation == IRArithmeticOperation.Mod) result.Operation = IRBinaryOperation.Mod;

            result.Left = LeftNode.To() as IRExpr;
            result.Right = RightNode.To() as IRExpr;

            return result;
        }

        public override IRExpression To(IRExpression from)
        {
            throw new NotImplementedException();
        }




        public ExprNode Add(ExprNode source, ExprNode target)
        {
            if (source.Type == null || target.Type == null) return null;

            if (source.Type.IsIntegerType()) IntegerAdd(source, target);
            else if (source.Type.DataType == AJDataType.Double) FloatingAdd(source, target);
            else if (source.Type.DataType == AJDataType.String) StringAdd(source, target);

            return this;
        }


        public ExprNode Sub(ExprNode source, ExprNode target)
        {
            if (source.Type == null || target.Type == null) return null;

            if (source.Type.IsIntegerType()) IntegerSub(source, target);
            else if (source.Type.DataType == AJDataType.Double) FloatingSub(source, target);

            return this;
        }

        public ExprNode Mul(ExprNode source, ExprNode target)
        {
            if (source.Type == null || target.Type == null) return null;

            if (source.Type.IsIntegerType()) IntegerMul(source, target);
            else if (source.Type.DataType == AJDataType.Double) FloatingMul(source, target);

            return this;
        }

        public ExprNode Div(ExprNode source, ExprNode target)
        {
            if (source.Type == null || target.Type == null) return null;

            if (source.Type.IsIntegerType()) IntegerDiv(source, target);
            else if (source.Type.DataType == AJDataType.Double) FloatingDiv(source, target);

            return this;
        }

        public ExprNode Mod(ExprNode source, ExprNode target)
        {
            if (source.Type == null || target.Type == null) return null;

            if (source.Type.IsIntegerType()) IntegerMod(source, target);
            else if (source.Type.DataType == AJDataType.Double) FloatingMod(source, target);

            return this;
        }


        /*******************************************************/
        /// <summary>
        /// This function means the expression for add when the source is Integer type.
        /// </summary>
        /// <remarks>
        /// basis condition: The ValueState of source and target is the Fixed.
        /// </remarks>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        /*******************************************************/
        private ExprNode IntegerAdd(ExprNode source, ExprNode target)
        {
            if (target.Type.IsIntegerType())
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (int)source.Value + (int)target.Value;
                ValueState = State.Fixed;
            }
            else if (target.Type.IsFloatingType())
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (int)source.Value + (double)target.Value;
                ValueState = State.Fixed;
            }
            else if (target.Type.DataType == AJDataType.String)
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (int)source.Value + (string)target.Value;
                ValueState = State.Fixed;
            }

            return this;
        }


        /*******************************************************/
        /// <summary>
        /// This function means the expression for add when the source is floating type.
        /// </summary>
        /// <remarks>
        /// basis condition: The ValueState of source and target is the Fixed.
        /// </remarks>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        /*******************************************************/
        private ExprNode FloatingAdd(ExprNode source, ExprNode target)
        {
            if (target.Type.IsArithmeticType())
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (double)source.Value + (double)target.Value;
                ValueState = State.Fixed;
            }
            else if (target.Type.DataType == AJDataType.String)
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (double)source.Value + (string)target.Value;
                ValueState = State.Fixed;
            }

            return this;
        }


        /*******************************************************/
        /// <summary>
        /// This function means the expression for add when the source is string type.
        /// </summary>
        /// <remarks>
        /// basis condition: The ValueState of source and target is the Fixed.
        /// </remarks>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        /*******************************************************/
        private ExprNode StringAdd(ExprNode source, ExprNode target)
        {
            if (target.Type.IsArithmeticType())
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (string)source.Value + (double)target.Value;
                ValueState = State.Fixed;
            }
            else if (target.Type.DataType == AJDataType.String)
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (string)source.Value + (string)target.Value;
                ValueState = State.Fixed;
            }

            return this;
        }


        /*******************************************************/
        /// <summary>
        /// This function means the expression for sub when the source is Integer type.
        /// </summary>
        /// <remarks>
        /// basis condition: The ValueState of source and target is the Fixed.
        /// </remarks>
        /// <param name="target"></param>
        /// <returns></returns>
        /*******************************************************/
        private ExprNode IntegerSub(ExprNode source, ExprNode target)
        {
            if (target.Type.IsIntegerType())
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (int)source.Value - (int)target.Value;
                ValueState = State.Fixed;
            }
            else if (target.Type.IsFloatingType())
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (int)source.Value - (double)target.Value;
                ValueState = State.Fixed;
            }

            return this;
        }


        /*******************************************************/
        /// <summary>
        /// This function means the expression for sub when the source is floating type.
        /// </summary>
        /// <remarks>
        /// basis condition: The ValueState of source and target is the Fixed.
        /// </remarks>
        /// <param name="target"></param>
        /// <returns></returns>
        /*******************************************************/
        private ExprNode FloatingSub(ExprNode source, ExprNode target)
        {
            if (target.Type.IsArithmeticType())
            {
                Value = (double)source.Value - (double)target.Value;
                ValueState = State.Fixed;
            }
            else Type = null;

            return this;
        }


        /*******************************************************/
        /// <summary>
        /// This function means the expression for mul when the source is Integer type.
        /// </summary>
        /// <remarks>
        /// basis condition: The ValueState of source and target is the Fixed.
        /// </remarks>
        /// <param name="target"></param>
        /// <returns></returns>
        /*******************************************************/
        private ExprNode IntegerMul(ExprNode source, ExprNode target)
        {
            if (target.Type.IsIntegerType())
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (int)source.Value * (int)target.Value;
                ValueState = State.Fixed;
            }
            else if (target.Type.IsFloatingType())
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (int)source.Value * (double)target.Value;
                ValueState = State.Fixed;
            }

            return this;
        }


        /*******************************************************/
        /// <summary>
        /// This function means the expression for mul when the source is floating type.
        /// </summary>
        /// <remarks>
        /// basis condition: The ValueState of source and target is the Fixed.
        /// </remarks>
        /// <param name="target"></param>
        /// <returns></returns>
        /*******************************************************/
        private ExprNode FloatingMul(ExprNode source, ExprNode target)
        {
            if (target.Type.IsArithmeticType())
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (double)source.Value * (double)target.Value;
                ValueState = State.Fixed;
            }

            return this;
        }


        /*******************************************************/
        /// <summary>
        /// This function means the expression for div when the source is Integer type.
        /// </summary>
        /// <remarks>
        /// basis condition: The ValueState of source and target is the Fixed.
        /// </remarks>
        /// <param name="target"></param>
        /// <returns></returns>
        /*******************************************************/
        private ExprNode IntegerDiv(ExprNode source, ExprNode target)
        {
            if (target.Type.IsIntegerType())
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (int)source.Value / (int)target.Value;
                ValueState = State.Fixed;
            }
            else if (target.Type.IsFloatingType())
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (int)source.Value / (double)target.Value;
                ValueState = State.Fixed;
            }

            return this;
        }


        /*******************************************************/
        /// <summary>
        /// This function means the expression for div when the source is floating type.
        /// </summary>
        /// <remarks>
        /// basis condition: The ValueState of source and target is the Fixed.
        /// </remarks>
        /// <param name="target"></param>
        /// <returns></returns>
        /*******************************************************/
        private ExprNode FloatingDiv(ExprNode source, ExprNode target)
        {
            if (target.Type.IsArithmeticType())
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (double)source.Value / (double)target.Value;
                ValueState = State.Fixed;
            }

            return this;
        }


        /*******************************************************/
        /// <summary>
        /// This function means the expression for mod when the source is integer type.
        /// </summary>
        /// <remarks>
        /// basis condition: The ValueState of source and target is the Fixed.
        /// </remarks>
        /// <param name="target"></param>
        /// <returns></returns>
        /*******************************************************/
        private ExprNode IntegerMod(ExprNode source, ExprNode target)
        {
            if (target.Type.IsIntegerType())
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (int)source.Value % (int)target.Value;
                ValueState = State.Fixed;
            }
            else if (target.Type.IsFloatingType())
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (int)source.Value % (double)target.Value;
                ValueState = State.Fixed;
            }

            return this;
        }


        /*******************************************************/
        /// <summary>
        /// This function means the expression for mod when the source is floating type.
        /// </summary>
        /// <remarks>
        /// basis condition: The ValueState of source and target is the Fixed.
        /// </remarks>
        /// <param name="target"></param>
        /// <returns></returns>
        /*******************************************************/
        private ExprNode FloatingMod(ExprNode source, ExprNode target)
        {
            if (target.Type.IsArithmeticType())
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (double)source.Value % (double)target.Value;
                ValueState = State.Fixed;
            }

            return this;
        }
    }
}
