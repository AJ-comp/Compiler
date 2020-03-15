using Parse.Extensions;
using Parse.FrontEnd.Parsers.ErrorHandling.GrammarPrivate;
using Parse.FrontEnd.Parsers.EventArgs;
using Parse.FrontEnd.Parsers.Properties;
using Parse.FrontEnd.RegularGrammar;
using Parse.Tokenize;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Parse.FrontEnd.Parsers.Datas
{
    /// <summary>
    /// 
    /// </summary>
    /// <see cref="https://www.lucidchart.com/documents/edit/c96f0bde-4111-4957-bf65-75b56d8074dc/0_0?beaconFlowId=687BBA49A656D177"/>
    public class ParsingBlock
    {
        internal List<ParsingUnit> units = new List<ParsingUnit>();
        public TokenData Token { get; } = null;
        public IReadOnlyList<ParsingUnit> Units => this.units;
        public IReadOnlyList<ParsingUnit> ErrorUnits
        {
            get
            {
                List<ParsingUnit> result = new List<ParsingUnit>();

                Parallel.ForEach(this.units, (unit) => 
                {
                    if (unit.IsError)
                    {
                        lock(result) result.Add(unit);
                    }
                });

                return result;
            }
        }

        public ParsingBlock(TokenData token)
        {
            this.Token = token;
        }

        /// <summary>
        /// This constructor creates instance has parsingUnit and token.
        /// </summary>
        /// <param name="parsingUnit"></param>
        /// <param name="token"></param>
        public ParsingBlock(ParsingUnit parsingUnit, TokenData token) : this(token)
        {
            this.units.Add(parsingUnit);
        }

        public ParsingBlock(IEnumerable<ParsingUnit> parsingUnits, TokenData token) : this(token)
        {
            this.units = new List<ParsingUnit>(parsingUnits);
        }

        /// <summary>
        /// This function returns the value after adding a new parsing item on the current parsing block.
        /// </summary>
        public ParsingUnit AddParsingItem()
        {
            if (this.units.Count == 0) this.units.Add(ParsingUnit.FirstParsingUnit);
            else this.units.Add(new ParsingUnit(this.Units.Last().AfterStack));

            return this.units.Last();
        }

        /// <summary>
        /// This function creates ParsingBlock that has ParsingUnit that next to prevParsingUnit.
        /// </summary>
        /// <param name="prevParsingUnit"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static ParsingBlock CreateNextParsingBlock(ParsingUnit prevParsingUnit, TokenData token)
        {
            var result = new ParsingBlock(token);

            ParsingUnit unitToAdd = (prevParsingUnit == null) ? ParsingUnit.FirstParsingUnit : new ParsingUnit(prevParsingUnit.AfterStack);
            result.units.Add(unitToAdd);

            return result;
        }

        public override string ToString() => string.Format("TokenCell : {0}, Unit count : {1}", this.Token, this.units.Count);
    }

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
        public string ErrorMessage { get; private set; }
        public ErrorPosition ErrorPosition { get; private set; } = ErrorPosition.OnNormalToken;

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

        public ParsingUnit(Stack<object> beforeStack)
        {
            BeforeStack = beforeStack;
        }

        /// <summary>
        /// This function copy before stack data to the after stack.
        /// </summary>
        public void CopyBeforeStackToAfterStack()
        {
            this.AfterStack = this.BeforeStack.Clone();
        }

        public void ChangeToFailedState(ErrorHandler errorHandler = null) => this.ChangeToFailedState(Resource.CantShift + " " + this.PossibleTerminalSet + " " + Resource.MustCome, errorHandler);

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

        public void ChangeToNormalState()
        {
            this.IsError = false;
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
