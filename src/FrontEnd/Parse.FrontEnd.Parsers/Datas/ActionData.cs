using AJ.Common.Helpers;
using Janglim.FrontEnd.RegularGrammar;
using System.Collections.Generic;
using System.Linq;
using static Janglim.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Janglim.FrontEnd.Parsers.Datas
{
    public class ActionData
    {
        public ActionDir Direction { get; internal set; } = ActionDir.NotProcessed;
        public object Dest { get; internal set; } = null;

        public ActionData() { }

        public ActionData(ActionDir actionDir, object actionDest)
        {
            this.Direction = actionDir;
            this.Dest = actionDest;
        }

        /// <summary>
        /// A strongly-typed projection of this action (<see cref="Direction"/> + <see cref="Dest"/>):
        /// <see cref="ParseAction.Shift"/> / <see cref="ParseAction.Goto"/> / <see cref="ParseAction.Reduce"/> /
        /// <see cref="ParseAction.Accept"/>, or <c>null</c> for a non-parse direction (NotProcessed / Failed).
        /// Additive and read-only — it does not change Direction/Dest, so existing consumers are
        /// unaffected. (For a Failed action the Dest is an error handler, so it maps to null here.)
        /// </summary>
        public ParseAction Action
        {
            get
            {
                switch (Direction)
                {
                    case ActionDir.Shift:         return new ParseAction.Shift((int)Dest);
                    case ActionDir.Goto:          return new ParseAction.Goto((int)Dest);
                    case ActionDir.Reduce:        return new ParseAction.Reduce((NonTerminalSingle)Dest);
                    case ActionDir.EpsilonReduce: return new ParseAction.Reduce((NonTerminalSingle)Dest, true);
                    case ActionDir.Accept:        return new ParseAction.Accept((NonTerminalSingle)Dest);
                    default:                      return null; // NotProcessed / Failed
                }
            }
        }

        public override string ToString()
        {
            var destString = (Dest == null) ? "null" : Dest.ToString();

            return $"{Direction.ToDescription()} {destString}";
        }
    }


    public class ActionDataList : List<ActionData>
    {
        public ActionDir DefaultDirection => (Count == 0) ? ActionDir.Failed : this.First().Direction;
        public object DefaultDest => (Count == 0) ? ActionDir.Failed : this.First().Dest;
    }
}
