using Parse.FrontEnd.Ast;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.MiddleEnd.IR.Expressions;
using System.Collections.Generic;
using Parse.Extensions;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes
{
    public class UsingStNode : StatementNode
    {
        public List<TokenData> NameTokens { get; set; } = new List<TokenData>();
        public string FullName => NameTokens.ItemsString(PrintType.String, string.Empty, ".");

        public UsingStNode(AstSymbol node) : base(node)
        {
        }

        // [0:n] = Ident chin [TerminalNode List]
        public override SdtsNode Compile(CompileParameter param)
        {
            base.Compile(param);

            foreach (var item in Items)
            {
                var node = item.Compile(param) as TerminalNode;
                NameTokens.Add(node.Token);
            }

            string name = string.Empty;
            foreach (var nameToken in NameTokens)
            {
                name += nameToken.Input;
                if (!NamespaceDictionary.Instance.ContainsKey(name))
                    Alarms.Add(AJAlarmFactory.CreateAJ0031(nameToken));

                name += ".";
            }

            return this;
        }

        public override IRExpression To()
        {
            throw new System.NotImplementedException();
        }

        public override IRExpression To(IRExpression from)
        {
            throw new System.NotImplementedException();
        }
    }
}
