using AJ.Common;
using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.Types;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes
{
    public abstract class ExprNode : AJNode, IHasType, IExportable<IRExpression>, ICanbeStatement
    {
        public object Value { get; set; } = null;
        public State ValueState { get; set; } = State.NotFixed;
        public AJType Type { get; internal set; } = null;

        public bool AlwaysTrue { get; } = false;

        public bool IsRoot => !(Parent is ExprNode);


        protected ExprNode(AstSymbol node) : base(node)
        {
        }


        public abstract IRExpression To();
        public abstract IRExpression To(IRExpression from);


        public ExprNode Assign(ExprNode source, ExprNode target)
        {
            if (source.Type.DataType == AJDataType.Bool) return BoolAssign(source, target);
            else if (source.Type.IsArithmeticType()) return ArithmeticAssign(source, target);

            return source;
        }

        protected void AddAlarmUnknownType(TokenData token)
        {
            Alarms.Add(new MeaningErrInfo(token, nameof(AlarmCodes.AJ0041), AlarmCodes.AJ0041));
        }

        protected void AddAlarmUnknownType(IEnumerable<TokenData> tokens)
        {
            Alarms.Add(new MeaningErrInfo(tokens, nameof(AlarmCodes.AJ0041), AlarmCodes.AJ0041));
        }

        private ExprNode BoolAssign(ExprNode source, ExprNode target)
        {
            if (target.Type.DataType == AJDataType.Bool)
            {
                source.Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                source.Value = (bool)target.Value;
                source.ValueState = State.Fixed;
                return source;
            }

            Alarms.Add(AJAlarmFactory.CreateMCL0025(target, source));
            return null;
        }


        private ExprNode ArithmeticAssign(ExprNode source, ExprNode target)
        {
            if (source.Type.DataType == target.Type.DataType ||
                source.Type.IsIncludeType(target.Type))
            {
                Type = source.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                source.Value = (bool)target.Value;
                source.ValueState = State.Fixed;

                return source;
            }

            if (!target.Type.IsArithmeticType())
            {
                Alarms.Add(AJAlarmFactory.CreateMCL0025(target, source));
                return null;
            }

            if (target.Type.IsIncludeType(source.Type))
            {
                Alarms.Add(AJAlarmFactory.CreateAJ0048(target, source));
                return null;
            }

            // etc case
            Alarms.Add(AJAlarmFactory.CreateMCL0025(target, source));
            return null;
        }
    }
}
