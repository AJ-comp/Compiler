using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes
{
    public class UseMemberNode : ExprNode
    {
        public UseMemberNode(AstSymbol node) : base(node)
        {
        }

        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            TypeDefNode lastTypeNode = GetParent(typeof(TypeDefNode)) as TypeDefNode;
            FuncDefNode funcDefNode = GetParent(typeof(FuncDefNode)) as FuncDefNode;

            if (funcDefNode.Type.Static)
            {
                AddAlarmDoNotUseThis((Items[0].Compile(param) as TerminalNode).Token);
                return this;
            }

            int offset = 1;
            TokenDataList seenTokens = new TokenDataList();
            while (true)
            {
                if (offset >= Items.Count) break;
                if (lastTypeNode == null)
                {
                    AddAlarmUnknownType(seenTokens);
                    break;
                }
                if (Items[offset] is CallNode) param.ParentNode = lastTypeNode;

                var node = Items[offset++].Compile(param);
                if (node is TerminalNode)
                {
                    var tNode = node as TerminalNode;
                    seenTokens.Add(tNode.Token);

                    var matchField = lastTypeNode.AllFields.Where(x => x.Name == tNode.Token.Input).FirstOrDefault();
                    if (matchField == null)
                    {
                        AddAlarmNoExistField(lastTypeNode, tNode.Token);
                        return this;
                    }

                    lastTypeNode = matchField.Type.DefineNode;
                }
                else if (node is CallNode)
                {
                    var tNode = node as CallNode;
                    seenTokens.Add(tNode.MethodNameToken);

                    lastTypeNode = tNode.Type.DefineNode;
                }
            }

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


        private void AddAlarmDoNotUseThis(TokenData thisToken)
        {
            Alarms.Add(new MeaningErrInfo(thisToken, nameof(AlarmCodes.AJ0042), AlarmCodes.AJ0042));
        }

        private void AddAlarmNoExistField(TypeDefNode type, TokenData field)
        {
            Alarms.Add(new MeaningErrInfo(field,
                                                            nameof(AlarmCodes.AJ0043),
                                                            string.Format(AlarmCodes.AJ0043, type.FullName, field.Input)));
        }


        private void AddAlarmUnknownType(IEnumerable<TokenData> tokens)
        {
            Alarms.Add(new MeaningErrInfo(tokens, nameof(AlarmCodes.AJ0046), AlarmCodes.AJ0046));
        }
    }
}
