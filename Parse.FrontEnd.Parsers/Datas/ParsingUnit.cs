using Parse.Extensions;
using Parse.FrontEnd.Parsers.ErrorHandling.GrammarPrivate;
using Parse.FrontEnd.Parsers.EventArgs;
using Parse.FrontEnd.Parsers.Properties;
using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd.Parsers.Datas
{
    /// <summary>
    /// 
    /// </summary>
    /// <see cref="https://www.lucidchart.com/documents/edit/c96f0bde-4111-4957-bf65-75b56d8074dc/0_0?beaconFlowId=687BBA49A656D177"/>
    public class ParsingUnit
    {
        public Stack<object> BeforeStack { get; } = new Stack<object>();
        public Stack<object> AfterStack { get; internal set; } = new Stack<object>();
        public ActionData Action { get; internal set; } = new ActionData();
        public TokenData InputValue { get; internal set; }
        public TerminalSet PossibleTerminalSet { get; internal set; } = new TerminalSet();


        public bool IsError { get; private set; } = false;
        public ErrorHandler ErrorHandler { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;
        public ErrorPosition ErrorPosition { get; private set; } = ErrorPosition.OnNormalToken;

        public bool IsRecovery { get; private set; } = false;
        public string RecoveryMessage { get; private set; } = string.Empty;


        /// <summary>
        /// This property creates a ParsingUnit instance with initial stack state.
        /// </summary>
        public static ParsingUnit FirstParsingUnit
        {
            get
            {
                var result = new ParsingUnit();
                result.BeforeStack.Push(0);

                return result;
            }
        }

        public ParsingUnit() { }

        public ParsingUnit(TokenData token)
        {
            InputValue = token;
        }

        public ParsingUnit(Stack<object> beforeStack)
        {
            BeforeStack = beforeStack;
        }

        public ParsingUnit(Stack<object> beforeStack, Stack<object> afterStack)
        {
            BeforeStack = beforeStack;
            AfterStack = afterStack;
        }

        public ParsingUnit(Stack<object> beforeStack, Stack<object> afterStack, TokenData token) : this(token)
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

        public void ChangeToFailedState(ErrorHandler errorHandler = null) => this.ChangeToFailedState("(" + Resource.CantShift + " " + this.PossibleTerminalSet + " " + Resource.MustCome + ")", errorHandler);

        public void ChangeToFailedState(string errorMessage, ErrorHandler errorHandler = null)
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

        public void SetRecoveryMessage(string recoveryMessage)
        {
            this.IsError = true;
            this.IsRecovery = true;
            this.RecoveryMessage = recoveryMessage;
        }

        public void ChangeToNormalState()
        {
            this.IsError = false;
            this.IsRecovery = false;
            this.ErrorHandler = null;
            this.ErrorMessage = string.Empty;
        }

        public override string ToString()
        {
            var inputString = (InputValue == null) ? "null" : this.InputValue.Input;

            return string.Format("BeforeStack count : {0}, AfterStack count : {1}, InputValue : {2}, Action : {3}", BeforeStack.Count, AfterStack.Count, inputString, Action.ToString());
        }
    }
}
