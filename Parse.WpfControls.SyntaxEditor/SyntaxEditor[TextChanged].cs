using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Tokenize;
using System;
using System.Linq;
using System.Windows.Controls;

namespace Parse.WpfControls.SyntaxEditor
{
    public partial class SyntaxEditor
    {
        private void TextArea_TextChanged(object sender, TextChangedEventArgs e)
        {
            Text = TextArea.Text;
            UpdateTokenInfos(e.Changes.First());

            // shallow copy
            lock (_lockObject)
                _csPostProcessData = new Tuple<ParsingResult, TextChange>(parsingResult, e.Changes.First());

            this.TextArea.InvalidateVisual();
        }


        private void Compiler_LexingCompleted(object sender, LexingData e)
        {
            TextArea.RecentLexedData = e;
        }

        private void Compiler_ParsingCompleted(object sender, ParsingResult e)
        {
            parsingResult = e.Clone() as ParsingResult;
        }


        private void UpdateTokenInfos(TextChange changeInfo)
        {
            if (changeInfo.RemovedLength > 0 && changeInfo.AddedLength > 0) this.UpdateTokens(changeInfo);
            else if (changeInfo.RemovedLength > 0)
                this.Compiler.Operate(FileName, changeInfo.Offset, changeInfo.RemovedLength);
            else if (changeInfo.AddedLength > 0)
            {
                string addString = this.Text.Substring(changeInfo.Offset, changeInfo.AddedLength);
                this.Compiler.Operate(FileName, changeInfo.Offset, addString);
            }
        }

        private void UpdateTokens(TextChange changeInfo)
        {
            this.Compiler.Operate(FileName, changeInfo.Offset, changeInfo.RemovedLength);

            string addString = this.Text.Substring(changeInfo.Offset, changeInfo.AddedLength);
            this.Compiler.Operate(FileName, changeInfo.Offset, addString);
        }


        private ParsingResult parsingResult = new ParsingResult();
    }
}
