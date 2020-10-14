using Parse.FrontEnd;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Parse.WpfControls.SyntaxEditor
{
    public partial class SyntaxEditor
    {
        private void MoveCaretToToken(int tokenIndex, TokenData tokenData)
        {
            var viewTokens = TextArea.RecentLexedData.TokensForView;

            if (tokenIndex >= viewTokens.Count) return;
            var tokenToCompare = viewTokens[tokenIndex];

            if (tokenToCompare != tokenData.TokenCell) return;

            this.TextArea.MoveCaretToToken(tokenIndex);
        }



        private static void ParsingFailedListPreProcess(ParsingResult e)
        {
            List<Tuple<int, ParsingBlock>> errorBlocks = new List<Tuple<int, ParsingBlock>>();

            Parallel.For(0, e.Count, i =>
            {
                var block = e[i];
                if (block.ErrorInfos.Count == 0)
                {
                    block.Token.TokenCell.ValueOptionData = DrawingOption.None;
                    return;
                }

                var errToken = block.Token;

                DrawingOption status = DrawingOption.None;
                if (errToken.TokenCell.ValueOptionData != null)
                    status = (DrawingOption)errToken.TokenCell.ValueOptionData;

                if (errToken.Kind == new EndMarker())
                    status |= DrawingOption.EndPointUnderline;
                else
                    status |= DrawingOption.Underline;

                errToken.TokenCell.ValueOptionData = status;

                lock (errorBlocks) errorBlocks.Add(new Tuple<int, ParsingBlock>(i, block));
            });
        }
    }
}
