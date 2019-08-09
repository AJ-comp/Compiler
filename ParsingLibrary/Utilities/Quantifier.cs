using ParsingLibrary.Datas.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParsingLibrary.Utilities
{
    public class Quantifier
    {
        /// <summary>
        /// https://www.lucidchart.com/documents/edit/2332a179-9756-4e52-ac61-93f0d6d696b2/0
        /// </summary>
        /// <param name="symbol">Expression 형태로 변환할 symbol</param>
        /// <returns></returns>
        public static NonTerminal ZeroOrMore(Terminal symbol)
        {
            NonTerminal result = AutoGenerator.GetNonTerminal();
            NonTerminal bridge = AutoGenerator.GetNonTerminal();

            result.AddAsAlter(bridge, new Epsilon());
            bridge.AddAsConcat(symbol, result);

            return result;
        }

        /// <summary>
        /// https://www.lucidchart.com/documents/edit/62030320-0871-4549-95cc-980cff7ab9cf/2
        /// https://www.lucidchart.com/documents/edit/62030320-0871-4549-95cc-980cff7ab9cf/4
        /// </summary>
        /// <param name="contents"></param>
        /// <returns></returns>
        public static NonTerminal ZeroOrMore(NonTerminal contents)
        {
            NonTerminal result = AutoGenerator.GetNonTerminal();
            NonTerminal bridge = AutoGenerator.GetNonTerminal();

            result.AddAsAlter(bridge, new Epsilon());
            bridge.AddAsConcat(contents, result);

            return result;
        }

        /// <summary>
        /// https://www.lucidchart.com/documents/edit/2332a179-9756-4e52-ac61-93f0d6d696b2/1
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static NonTerminal OneOrMore(Terminal symbol)
        {
            NonTerminal result = AutoGenerator.GetNonTerminal();
            NonTerminal bridge = AutoGenerator.GetNonTerminal();

            result.AddAsConcat(symbol, bridge);
            bridge.AddAsAlter(result, new Epsilon());

            return result;
        }

        /// <summary>
        /// https://www.lucidchart.com/documents/edit/2332a179-9756-4e52-ac61-93f0d6d696b2/1
        /// </summary>
        /// <param name="contents"></param>
        /// <returns></returns>
        public static NonTerminal OneOrMore(NonTerminal contents)
        {
            NonTerminal result = AutoGenerator.GetNonTerminal();
            NonTerminal bridge = AutoGenerator.GetNonTerminal();

            result.AddAsConcat(contents, bridge);
            bridge.AddAsAlter(result, new Epsilon());

            return result;
        }

        /// <summary>
        /// https://www.lucidchart.com/documents/edit/2332a179-9756-4e52-ac61-93f0d6d696b2/2
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static NonTerminal Optional(Terminal symbol)
        {
            NonTerminal result = AutoGenerator.GetNonTerminal();

            result.AddAsAlter(symbol, new Epsilon());

            return result;
        }

        /// <summary>
        /// https://www.lucidchart.com/documents/edit/2332a179-9756-4e52-ac61-93f0d6d696b2/2
        /// </summary>
        /// <param name="contents"></param>
        /// <returns></returns>
        public static NonTerminal Optional(NonTerminal contents)
        {
            NonTerminal result = AutoGenerator.GetNonTerminal();

            result.AddAsAlter(contents, new Epsilon());

            return result;
        }
    }
}
