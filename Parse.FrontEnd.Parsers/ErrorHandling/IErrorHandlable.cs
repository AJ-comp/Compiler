﻿using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.Logical;

namespace Parse.FrontEnd.Parsers.ErrorHandling
{
    public interface IErrorHandlable
    {
        ErrorHandlingResult Call(ParserSnippet snippet, ParsingResult parsingResult, int seeingTokenIndex);
    }
}