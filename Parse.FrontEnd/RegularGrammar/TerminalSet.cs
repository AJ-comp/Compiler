using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Parse.FrontEnd.RegularGrammar
{
    public class TerminalSet : HashSet<Terminal>
    {
        public bool IsNull => this.Count == 0;
        public bool IsNullAble => this.Contains(new Epsilon());

        public TerminalSet() { }

        public TerminalSet(Terminal terminal)
        {
            this.Add(terminal);
        }

        public TerminalSet(TerminalSet terminalSet)
        {
            this.UnionWith(terminalSet);
        }

        public bool Contains(string value)
        {
            bool result = false;

            foreach(var item in this)
            {
                if(item.Value == value)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        public Terminal ContainFirst(string value)
        {
            foreach (var terminal in this)
            {
                if (terminal.IsWord)
                {
                    if (Regex.Match(value, terminal.Value).Value == value) return terminal;
                }
                if (terminal.Value == value) return terminal;
            }

            return new NotDefined();
        }

        public TerminalSet ContainSet(string value)
        {
            TerminalSet result = new TerminalSet();

            foreach(var terminal in this)
            {
                if(terminal.IsWord)
                {
                    if (Regex.Match(value, terminal.Value).Value == value) result.Add(terminal);
                }
                if (terminal.Value == value) result.Add(terminal);
            }

            return result;
        }

        public TerminalSet RingSum(Terminal param)
        {
            TerminalSet result = new TerminalSet(this);

            if (result.IsNull) result.Add(param);
            else if (result.IsNullAble)
            {
                result.Remove(new Epsilon());
                result.Add(param);
            }

            return result;
        }

        public TerminalSet RingSum(TerminalSet param)
        {
            TerminalSet result = new TerminalSet(this);

            if (result.IsNull) result.UnionWith(param);
            else if (result.IsNullAble)
            {
                result.Remove(new Epsilon());
                result.UnionWith(param);
            }

            return result;
        }

        public override string ToString()
        {
            string result = "{";

            foreach(var item in this)   result += item.ToString() + ",";

            return result.Substring(0, result.Length - 1) + "}";
        }
    }
}
