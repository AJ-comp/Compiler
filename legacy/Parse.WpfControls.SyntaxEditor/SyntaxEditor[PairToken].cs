using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace Parse.WpfControls.SyntaxEditor
{
    public partial class SyntaxEditor
    {
        public bool CanGeneratePair(TextChange changeInfo)
        {
            if (!IsInsertWord(changeInfo)) return false;

            int caretIndex = changeInfo.Offset + changeInfo.AddedLength;
            var insertWord = TextArea.Text[caretIndex];
            if (insertWord != '(' && insertWord != '{' && insertWord != '[') return false;

            return TextArea.IsLastVisibleTokenInLine(caretIndex);
        }
    }
}
