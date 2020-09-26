using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Tokenize;
using Parse.WpfControls.SyntaxEditor.EventArgs;
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

                int tokenIndex = -1;
                Dispatcher.Invoke(() =>
                {
                    tokenIndex = this.TextArea.GetTokenIndexForCaretIndex(this.TextArea.CaretIndex, RecognitionWay.Back);
                });

                var list = this.GetCompletionList(localResult, tokenIndex);

                Dispatcher.Invoke(() =>
                {
                    this.ShowIntellisense(localTextChange, list);
                }, DispatcherPriority.ApplicationIdle);

                Dispatcher.Invoke(() =>
                {
                    var result = Compiler.StartSemanticAnalysis(FileName);
                    if (result == null) return;

                    ParsingFailedListPreProcess(localResult);
                    this.ParsingCompleted?.Invoke(this, new ParsingCompletedEventArgs(localResult,
                                                                                                                        result.SdtsRoot,
                                                                                                                        result.AllNodes,
                                                                                                                        result.FiredException));
                });
            }
        }



        private Thread _workerManager;
    }
}
