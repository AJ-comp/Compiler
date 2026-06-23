using Parse.FrontEnd.Ast;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.MiddleEnd.IR.Expressions;
using System.Collections.Generic;
using Parse.Extensions;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes
{
    public class UsingStNode : StatementNode
    {
        public TokenDataList NameTokens { get; set; } = new TokenDataList();
        public string FullName => NameTokens.ItemsString(PrintType.String, string.Empty, ".");

        public UsingStNode(AstSymbol node) : base(node)
        {
        }

        // [0:n] = Ident chin [TerminalNode List]
        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            base.CompileLogic(param);

            foreach (var item in Items)
            {
                var node = item.Compile(param) as TerminalNode;
                NameTokens.Add(node.Token);
            }

            if (!SymbolTable.Instance.ContainsKey(NameTokens.ToListString()))
                Alarms.Add(AJAlarmFactory.CreateAJ0031(NameTokens));

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
