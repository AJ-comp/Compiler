using Parse.FrontEnd;
using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Parsers.Datas;
using System;
using System.Collections.Generic;

namespace Parse.WpfControls.SyntaxEditor.EventArgs
{
    public class ParsingCompletedEventArgs
    {
        public ParsingResult ParsingResult { get; }
        public AstSymbol RootAst { get; }
        public IReadOnlyList<AstSymbol> AllNodes { get; }
        public Exception FiredException { get; }

        public ParsingCompletedEventArgs(ParsingResult parsingResult, AstSymbol rootNode, IReadOnlyList<AstSymbol> allNodes, Exception firedException)
        {
            ParsingResult = parsingResult;
            RootAst = rootNode;
            AllNodes = allNodes;
            FiredException = firedException;
        }
    }
}
