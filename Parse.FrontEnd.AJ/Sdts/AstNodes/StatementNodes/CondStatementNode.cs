using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Binary;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LiteralNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Single;
using Parse.FrontEnd.Ast;
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


        // [0] : TerminalNode [if or while]
        // [1] : ExprNode
        // [2] : StatementNode [statement]
        public override SdtsNode Compile(CompileParameter param)
        {
            try
            {
                var node = Items[1].Compile(param) as ExprNode;
                if (node.Type.DataType != Data.AJDataType.Bool)
                {
                    Alarms.Add(AJAlarmFactory.CreateMCL0025(node.Type.Name, "bool"));
                    return this;
                }

                // build only bool type
                if (node is CompareNode) CompareCondition = node as CompareNode;
                else if (node is SLogicalNode)
                {
                    CompareCondition = CompareNode.From(node as SLogicalNode);
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

                TrueStatement = Items[2].Compile(param) as StatementNode;
            }
            catch(Exception)
            {

            }
            finally
            {
                if (param.Build) DBContext.Instance.Insert(this);
            }

            return this;
        }
    }
}
