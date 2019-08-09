﻿using ParsingLibrary.Datas.RegularGrammar;
using ParsingLibrary.Grammars;
using System;

namespace ParsingLibrary.Utilities
{
    internal class AutoGenerator
    {
        private static Grammar grammar = null;

        public static void SetData(Grammar grammar, UInt32 notDefinedKey, UInt32 epsilonKey, UInt32 markerSymbolKey)
        {
            AutoGenerator.grammar = grammar;

            AutoGenerator.NotDefinedKey = notDefinedKey;
            AutoGenerator.EpsilonKey = epsilonKey;
            AutoGenerator.EndMarkerKey = markerSymbolKey;
        }

        public static UInt32 NotDefinedKey { get; private set; }
        public static UInt32 EpsilonKey { get; private set; }
        public static UInt32 EndMarkerKey { get; private set; }

        /// <summary>
        /// Create auto-generated Nonterminal (must need 
        /// </summary>
        /// <returns></returns>
        public static NonTerminal GetNonTerminal()
        {
            return AutoGenerator.grammar.CreateAutoGeneratedNT();
        }
    }
}
