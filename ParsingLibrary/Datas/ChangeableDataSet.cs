using ParsingLibrary.Datas.RegularGrammar;
using System;
using System.Collections.Generic;

namespace ParsingLibrary.Datas
{
    /// <summary>
    /// key 외의 분화식에서 value가 포함되어 있으면 모두 key로 교체
    /// </summary>
    public class ChangeableDataSet : Dictionary<NonTerminal, HashSet<NonTerminal>>
    {
        public NonTerminal ContainsElementOfValue(NonTerminal elementOfValue)
        {
            NonTerminal result = null;

            foreach(var key in this.Keys)
            {
                foreach(var item in this[key])
                {
                    if(item == elementOfValue)
                    {
                        result = key;
                        break;
                    }
                }
            }

            return result;
        }


        public override string ToString()
        {
            string result = string.Empty;

            foreach(var key in this.Keys)
            {
                foreach(var value in this[key])
                    result += string.Format("from {0} -> to {1}", value, key) + Environment.NewLine;
            }

            return result;
        }
    }
}
