using Parse.FrontEnd.Parsers.EventArgs;
using Parse.FrontEnd.Parsers.Properties;
using Parse.FrontEnd.RegularGrammar;
using System.Diagnostics;

namespace Parse.FrontEnd.Parsers.Datas
{
    /// <summary>
    /// 
    /// </summary>
    /// <see cref="https://www.lucidchart.com/documents/edit/c96f0bde-4111-4957-bf65-75b56d8074dc/0_0?beaconFlowId=687BBA49A656D177"/>

    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public class ParsingUnit
    {

        public ParsingStackUnit BeforeStack { get; } = new ParsingStackUnit();
        public ParsingStackUnit AfterStack { get; set; } = new ParsingStackUnit();


        public ActionData Action { get; internal set; } = new ActionData();
        public TokenData InputValue { get; set; }
        public TerminalSet PossibleTerminalSet { get; internal set; } = new TerminalSet();


        public bool IsError { get; set; } = false;
        public IErrorHandlable ErrorHandler { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;
        public ErrorPosition ErrorPosition { get; private set; } = ErrorPosition.OnNormalToken;


        /// <summary>
        /// This property creates a ParsingUnit instance with initial stack state.
        /// </summary>
        public static ParsingUnit FirstParsingUnit
        {
            get
            {
                var result = new ParsingUnit();
                result.BeforeStack.Stack.Push(0);

                return result;
            }
        }

        public ParsingUnit() { }

        public ParsingUnit(TokenData token)
        {
            InputValue = token;
        }

        public ParsingUnit(ParsingStackUnit beforeStack)
        {
            BeforeStack = beforeStack;
        }

        public ParsingUnit(ParsingStackUnit beforeStack, ParsingStackUnit afterStack)
        {
            BeforeStack = beforeStack;
            AfterStack = afterStack;
        }

        public ParsingUnit(ParsingStackUnit beforeStack, ParsingStackUnit afterStack, TokenData token) : this(token)
        {
            BeforeStack = beforeStack;
            AfterStack = afterStack;
        }

        /// <summary>
        /// This function copy before stack data to the after stack.
        /// </summary>
        public void CopyBeforeStackToAfterStack()
        {
            this.AfterStack = this.BeforeStack.Clone();
        }

        public void ChangeToFailedState(IErrorHandlable errorHandler = null) 
            => this.ChangeToFailedState("(" + Resource.CantShift + " " + this.PossibleTerminalSet + " " + Resource.MustCome + ")", errorHandler);

        public void ChangeToFailedState(string errorMessage, IErrorHandlable errorHandler = null)
        {
            this.IsError = true;
            this.ErrorHandler = errorHandler;
            this.ErrorMessage = errorMessage;

            if (InputValue?.Kind == new EndMarker())
            {
                ErrorPosition = ErrorPosition.OnEndMarker;
            }
            else
            {
                ErrorPosition = ErrorPosition.OnNormalToken;
            }
        }


        private string DebuggerDisplay
        {
            get
            {
                var inputString = (InputValue == null) ? "null" : this.InputValue.Input;

                return string.Format("BeforeStack count : {0}, AfterStack count : {1}, InputValue : {2}, Action : {3}",
                                                BeforeStack.Stack.Count, AfterStack.Stack.Count, inputString, Action.ToString());
            }
        }
    }
}
