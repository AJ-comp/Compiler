using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Binary;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LiteralNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Single;
using Parse.FrontEnd.Ast;
using Parse.Types;
using System;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes
{
    public abstract class CondStatementNode : StatementNode
    {
        public CompareNode CompareCondition { get; protected set; }
        public StatementNode TrueStatement { get; protected set; }
        public StatementNode FalseStatement { get; protected set; }

        protected CondStatementNode(AstSymbol node) : base(node)
        {
        }


        /// <summary>
        /// format summary  <br/>
        /// [0] : TerminalNode [if or while]    <br/>
        /// [1] : ExprNode  <br/>
        /// [2] : StatementNode [statement] <br/>
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            base.CompileLogic(param);
            var node = Items[1].Compile(param) as ExprNode;
            TrueStatement = Items[2].Compile(param) as StatementNode;

            if (node.Type == null) return this;
            else if (node.Type.DataType != AJDataType.Bool)
            {
                Alarms.Add(AJAlarmFactory.CreateMCL0025(node, "bool"));
                return this;
            }

            // build only bool type
            if (node is CompareNode) CompareCondition = node as CompareNode;
            else if (node is SLogicalNode)
            {
                CompareCondition = (node as SLogicalNode).ToCompareNode();
                CompareCondition.Compile(param);  // this node has to build because new node.
            }
            else if (node is UseIdentNode)
            {
                CompareCondition = CompareNode.From(node as UseIdentNode);
                CompareCondition.Compile(param);  // this node has to build because new node.
            }
            else if (node is BoolLiteralNode)
            {
                CompareCondition = CompareNode.From(node as BoolLiteralNode);
                CompareCondition.Compile(param);  // this node has to build because new node.
            }
            else
            {
                throw new Exception();
            }

            CheckNeverOperateCode();
            ClarifyReturn = TrueStatement.ClarifyReturn;

            return this;
        }


        /// <summary>
        /// Checks if the code is accessable.
        /// </summary>
        protected void CheckNeverOperateCode()
        {
            if (CompareCondition.ValueState == State.Fixed && (bool)(CompareCondition.Value) == false)
                Alarms.Add(new MeaningErrInfo(AllTokens, nameof(AlarmCodes.AJ0034), AlarmCodes.AJ0034, ErrorType.Warning));
        }
    }
}
