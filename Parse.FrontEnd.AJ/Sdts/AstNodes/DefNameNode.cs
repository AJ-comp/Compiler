﻿using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public class DefNameNode : AJNode
    {
        public TokenData Token { get; private set; }

        public DefNameNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Compile(CompileParameter param)
        {
            base.Compile(param);

            var terminalNode = Items[0].Compile(param) as TerminalNode;
            Token = terminalNode.Token;

            if (Token.Kind == AJGrammar.This)
                Alarms.Add(new MeaningErrInfo(Token, nameof(AlarmCodes.AJ0035), AlarmCodes.AJ0035));

            return this;
        }
    }
}
