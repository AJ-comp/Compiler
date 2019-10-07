using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Parsers.EventArgs;
using Parse.FrontEnd.Parsers.LR;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using WpfApp.Models;
using WpfApp.Properties;

namespace WpfApp.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        public LRParser Parser { get; } = new SLRParser(new MiniCGrammar());
        public ObservableCollection<ParsingAlarmList> AlarmLists { get; } = new ObservableCollection<ParsingAlarmList>();

        private RelayCommand<TextChangedEventArgs> _cmdMouseDown;



        public RelayCommand<TextChangedEventArgs> CmdMouseDown
        {
            get
            {
                if (_cmdMouseDown == null)
                    _cmdMouseDown = new RelayCommand<TextChangedEventArgs>(this.ExecuteTextChanged);

                return _cmdMouseDown;
            }
        }

        public MainWindowViewModel()
        {
            this.Parser.ParsingFailed += Parser_ParsingFailed;
//            this.Parser.MatchFailed

            
        }

        private void ExecuteTextChanged(TextChangedEventArgs e)
        {

        }

        private void Parser_ParsingFailed(object sender, ParsingFailedEventArgs e)
        {
            var message = string.Format(AlarmCodes.CE0000, e.PossibleSet.ToString());

            ParsingAlarmList alarmList = new ParsingAlarmList(AlarmCodes.CE0000, message, "", "", "1");
            this.AlarmLists.Add(alarmList);
        }
    }
}
