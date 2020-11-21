using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Support.EventArgs;
using Parse.FrontEnd.Tokenize;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Parse.WpfControls.SyntaxEditor
{
    public partial class SyntaxEditor
    {
        private void StartBackgroundWorker()
        {
            _workerManager = new Thread(WokerManagerLogic)
            {
                IsBackground = true,
                Priority = ThreadPriority.Lowest
            };
            _workerManager.Start();
        }

        private void WokerManagerLogic()
        {
            ParsingResult localResult;
            TextChange localTextChange;

            while (true)
            {
                Thread.Sleep(50);

                lock (_lockObject)
                {
                    if (_csPostProcessData == null) continue;

                    localResult = _csPostProcessData.Item1;
                    localTextChange = _csPostProcessData.Item2;
                    _csPostProcessData = null;
                }

                int pTokenIndex = -1;
                Dispatcher.Invoke(() =>
                {
                    var vTokenIndex = TextArea.GetTokenIndexForCaretIndex(TextArea.CaretIndex, RecognitionWay.Back);
                    pTokenIndex = TextArea.RecentLexedData.GetNearestPIndexFromVIndex(vTokenIndex);
                });

                var list = this.GetCompletionList(localResult, pTokenIndex);

                Dispatcher.Invoke(() =>
                {
                    this.ShowIntellisense(localTextChange, list);
                }, DispatcherPriority.ApplicationIdle);

                Dispatcher.Invoke(() =>
                {
                    var result = Compiler.StartSemanticAnalysis(FileName);
                    ParsingFailedListPreProcess(localResult);
                    TextArea.InvalidateVisual();

                    this.ParsingCompleted?.Invoke(this, new ParsingCompletedEventArgs(TextArea.RecentLexedData, localResult, result));
                });
            }
        }



        private Thread _workerManager;
    }
}
