using AJ.Common.Helpers;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Binary
{
    public class BinBitwiseAssignNode : BinBitwiseLogicalNode, IAssignable
    {
        public BinBitwiseAssignNode(AstSymbol node, IRBitwiseOperation operation) : base(node, operation)
        {
        }

        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            base.CompileLogic(param);
            CheckAssignable();

            if (!IsCanParsing) return this;

            if (Operation == IRBitwiseOperation.LeftShift) LeftShift(LeftNode, RightNode);
            else if (Operation == IRBitwiseOperation.RightShift) RightShift(LeftNode, RightNode);
            else if (Operation == IRBitwiseOperation.BitAnd) BitAnd(LeftNode, RightNode);
            else if (Operation == IRBitwiseOperation.BitOr) BitOr(LeftNode, RightNode);

            Assign(LeftNode, RightNode);

            if(Type == null) Alarms.Add(AJAlarmFactory.CreateMCL0023(this, Operation.ToDescription()));
            if (RootNode.IsBuild) DBContext.Instance.Insert(this);

            return this;
        }

        public override IRExpression To()
        {
            throw new NotImplementedException();
        }

        public override IRExpression To(IRExpression from)
        {
            throw new NotImplementedException();
        }
    }
}
