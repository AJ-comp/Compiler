using Parse.FrontEnd.Parsers.Datas;
using Parse.WpfControls.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Parse.WpfControls.SyntaxEditor
{
    public partial class SyntaxEditor
    {
        private void InitializeCompletionList()
        {
            this.completionList = new CompletionList(this.TextArea);
            this.completionList.RegisterKey(CompletionItemType.Class, new KeyData(FindResource("ClassActive16Path") as BitmapImage,
                                                                                                                    FindResource("ClassInActive16Path") as BitmapImage,
                                                                                                                    string.Format(Properties.Resources.DisplayOnly, Properties.Resources.Class_)));
            this.completionList.RegisterKey(CompletionItemType.CodeSnipp, new KeyData(FindResource("CodeSnippetActive16Path") as BitmapImage,
                                                                                                                            FindResource("CodeSnippetInActive16Path") as BitmapImage,
                                                                                                                            string.Format(Properties.Resources.DisplayOnly, Properties.Resources.CodeSnippet)));
            this.completionList.RegisterKey(CompletionItemType.Delegate, new KeyData(FindResource("DelegateActive16Path") as BitmapImage,
                                                                                                                          FindResource("DelegateInActive16Path") as BitmapImage,
                                                                                                                          string.Format(Properties.Resources.DisplayOnly, Properties.Resources.Delegate)));
            this.completionList.RegisterKey(CompletionItemType.Enum, new KeyData(FindResource("EnumActive16Path") as BitmapImage,
                                                                                                                     FindResource("EnumInActive16Path") as BitmapImage,
                                                                                                                     string.Format(Properties.Resources.DisplayOnly, Properties.Resources.Enumerate)));
            this.completionList.RegisterKey(CompletionItemType.Event, new KeyData(FindResource("EventActive16Path") as BitmapImage,
                                                                                                                     FindResource("EventInActive16Path") as BitmapImage,
                                                                                                                     string.Format(Properties.Resources.DisplayOnly, Properties.Resources.Event)));
            this.completionList.RegisterKey(CompletionItemType.Field, new KeyData(FindResource("FieldActive16Path") as BitmapImage,
                                                                                                                    FindResource("FieldInActive16Path") as BitmapImage,
                                                                                                                    string.Format(Properties.Resources.DisplayOnly, Properties.Resources.Field_)));
            this.completionList.RegisterKey(CompletionItemType.Function, new KeyData(FindResource("FunctionActive16Path") as BitmapImage,
                                                                                                                         FindResource("FunctionInActive16Path") as BitmapImage,
                                                                                                                         string.Format(Properties.Resources.DisplayOnly, Properties.Resources.Function_)));
            this.completionList.RegisterKey(CompletionItemType.Interface, new KeyData(FindResource("InterfaceActive16Path") as BitmapImage,
                                                                                                                          FindResource("InterfaceInActive16Path") as BitmapImage,
                                                                                                                          string.Format(Properties.Resources.DisplayOnly, Properties.Resources.Interface_)));
            this.completionList.RegisterKey(CompletionItemType.Keyword, new KeyData(FindResource("KeywordActive16Path") as BitmapImage,
                                                                                                                          FindResource("KeywordInActive16Path") as BitmapImage,
                                                                                                                          string.Format(Properties.Resources.DisplayOnly, Properties.Resources.Keyword_)));
            this.completionList.RegisterKey(CompletionItemType.Namespace, new KeyData(FindResource("NamespaceActive16Path") as BitmapImage,
                                                                                                                             FindResource("NamespaceInActive16Path") as BitmapImage,
                                                                                                                             string.Format(Properties.Resources.DisplayOnly, Properties.Resources.Namespace)));
            this.completionList.RegisterKey(CompletionItemType.Property, new KeyData(FindResource("PropertyActive16Path") as BitmapImage,
                                                                                                                         FindResource("PropertyInActive16Path") as BitmapImage,
                                                                                                                         string.Format(Properties.Resources.DisplayOnly, Properties.Resources.Property_)));
            this.completionList.RegisterKey(CompletionItemType.Struct, new KeyData(FindResource("StructActive16Path") as BitmapImage,
                                                                                                                      FindResource("StructInActive16Path") as BitmapImage,
                                                                                                                      string.Format(Properties.Resources.DisplayOnly, Properties.Resources.Structure)));
        }


        private IEnumerable<ItemData> GetCompletionList(ParsingResult parsingResult, int tokenIndex)
        {
            List<ItemData> result = new List<ItemData>();
            if (tokenIndex < 0) return result;
            if (parsingResult.Count <= tokenIndex) return result;

            foreach (var item in parsingResult[tokenIndex].PossibleTerminalSet)
            {
                if (item.Meaning == false) continue;
                if (item.IsWordPattern) continue;

                result.Add(new ItemData(CompletionItemType.Keyword, item.Value, string.Empty));
            }

            return result;
        }

        private bool IsBackSpace(TextChange changeInfo) => (changeInfo.RemovedLength >= 1 && changeInfo.AddedLength == 0);

        private void ShowIntellisense(TextChange changeInfo, IEnumerable<ItemData> items)
        {
            if (items.Count() == 0) { completionList.Close(); return; }

            var addString = TextArea.Text.Substring(changeInfo.Offset, changeInfo.AddedLength);
            if (addString.Length > 1) { completionList.Close(); return; }
            if (CloseCharacters.Contains(addString)) { completionList.Close(); return; }

            if (IsBackSpace(changeInfo))
            {
                if (TextArea.CaretIndex <= completionList.CaretIndexWhenCLOccur) { completionList.Close(); return; }
            }
            else if (completionList.IsOpened == false)
            {
                if (addString.Length == 1)
                {
                    completionList.CaretIndexWhenCLOccur = TextArea.CaretIndex - 1;
                }
            }

            var rect = TextArea.GetRectFromCharacterIndex(TextArea.CaretIndex);
            double x = rect.X;
            double y = (LineHeight > 0) ? rect.Y + LineHeight : rect.Y + TextArea.FontSize;
            var inputString = TextArea.Text.Substring(completionList.CaretIndexWhenCLOccur, TextArea.CaretIndex - completionList.CaretIndexWhenCLOccur);

            if (completionList.IsOpened) completionList.Show(inputString, x, y);
            else completionList.Create(items, x, y);
        }



        private CompletionList completionList;
    }
}
