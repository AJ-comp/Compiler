﻿using Parse;
using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Parse.FrontEnd.Grammars
{
    public abstract class Grammar
    {
        protected KeyManager keyManager = new KeyManager();

        protected Terminal space = new Terminal(TokenType.Delimiter, " ");
        protected Terminal tab = new Terminal(TokenType.Delimiter, "\t");
        protected Terminal lineFeed = new Terminal(TokenType.Delimiter, "\r");
        protected Terminal lineBreak = new Terminal(TokenType.Delimiter, "\n");

        /// <summary> first is character, second means whether ignore. true is ignoring. </summary>
        public Dictionary<string, bool> DelimiterDic { get; } = new Dictionary<string, bool>();

        public HashSet<NonTerminal> NonTerminalMultiples { get; } = new HashSet<NonTerminal>();
        public HashSet<NonTerminal> AutoGenerateNTSet { get; } = new HashSet<NonTerminal>();
        public HashSet<NonTerminalSingle> NonTerminalSingles { get; } = new HashSet<NonTerminalSingle>();
        public HashSet<ScopeInfo> ScopeInfos { get; } = new HashSet<ScopeInfo>();

        public TerminalSet TerminalSet { get; } = new TerminalSet();
        public NonTerminal StartSymbol { get; private set; } = null;

        public abstract Sdts SDTS { get; }


        public string IdentPattern { get; } = "[_a-zA-Z][_a-zA-Z0-9]*";
        public string LetterPattern { get; } = "[a-zA-Z]";
        public string VarLetterPattern { get; } = "[_a-zA-Z]";
        public string DigitPattern { get; } = "[0-9]+";

        public string EbnfExpression
        {
            get
            {
                string result = string.Empty;

                foreach (var symbol in this.NonTerminalMultiples) result += symbol.ToGrammarString() + Environment.NewLine;

                return result;
            }
        }

        public Grammar()
        {
//            this.Tokens.IdPattern = varLetter + "[_a-zA-Z0-9]*";
//            this.Tokens.LineCommentPattern = "//";
//            this.Tokens.BlockCommentPattern = "";

            this.DelimiterDic.Add(";", false);
            this.DelimiterDic.Add(",", false);
            this.DelimiterDic.Add(".", false);
            this.DelimiterDic.Add("=", false);
            this.DelimiterDic.Add("+", false);
            this.DelimiterDic.Add("-", false);
            this.DelimiterDic.Add("*", false);
            this.DelimiterDic.Add("/", false);
            this.DelimiterDic.Add("&", false);
            this.DelimiterDic.Add("|", false);
            this.DelimiterDic.Add("!", false);
            this.DelimiterDic.Add("(", false);
            this.DelimiterDic.Add(")", false);
            this.DelimiterDic.Add("[", false);
            this.DelimiterDic.Add("]", false);
            this.DelimiterDic.Add("{", false);
            this.DelimiterDic.Add("}", false);
            this.DelimiterDic.Add("?", false);
            this.DelimiterDic.Add("$", false);
            this.DelimiterDic.Add(" ", true);
            this.DelimiterDic.Add("\t", true);
            this.DelimiterDic.Add("\r", true);
            this.DelimiterDic.Add("\n", true);

            this.SetDataToHashSet();

            foreach (var terminal in this.TerminalSet)
            {
                if (terminal.TokenType != TokenType.Operator) continue;
                if (this.DelimiterDic.ContainsKey(terminal.Value)) continue;

                this.DelimiterDic.Add(terminal.Value, false);
            }
        }

        private void RegistAutoGenerateNT(NonTerminal nonTerminal)
        {
            this.NonTerminalMultiples.Add(nonTerminal);
            this.AutoGenerateNTSet.Add(nonTerminal);
        }

        private void SetDataToHashSet()
        {
            Type type = this.GetType();

            BindingFlags Flags = BindingFlags.Instance
                                           | BindingFlags.GetField
                                           | BindingFlags.SetField
                                           | BindingFlags.NonPublic;

            this.TerminalSet.Add(new EndMarker());
            foreach (var member in type.GetFields(Flags))
            {
                if (member.FieldType.FullName == typeof(Terminal).ToString())
                {
                    Terminal param = member.GetValue(this) as Terminal;

                    this.keyManager.AllocateUniqueKey(param);
                    this.TerminalSet.Add(param);
                }
                else if (member.FieldType.FullName == typeof(NonTerminal).ToString())
                {
                    NonTerminal param = member.GetValue(this) as NonTerminal;

                    this.keyManager.AllocateUniqueKey(param);
                    this.NonTerminalMultiples.Add(param);
                    if (param.AutoGenerated) this.AutoGenerateNTSet.Add(param);
                }
            }

            foreach (var multipleNT in this.NonTerminalMultiples)
            {
                if (multipleNT.IsStartSymbol) this.StartSymbol = multipleNT;

                foreach (var singleNT in multipleNT) this.NonTerminalSingles.Add(singleNT as NonTerminalSingle);
            }

            // 문법 할당 시 AutoGenerator를 통해 자동생성되는 비단말심벌들을 관리하기 위한 세팅
            AutoGenerator.SetKeyManager(this.keyManager);
            AutoGenerator.CreateAutoGenerateNT = this.RegistAutoGenerateNT;
        }

        /// <summary>
        /// Delete the not referenced all symbols
        /// </summary>
        public void DelNotRefAllSymbols()
        {
            HashSet<NonTerminal> refSet = new HashSet<NonTerminal>();

            foreach (var item in this.NonTerminalMultiples) refSet.UnionWith(item.ToNonTerminalSet());
            var notRefSet = this.NonTerminalMultiples.Except(refSet).ToHashSet();

            this.NonTerminalMultiples.ExceptWith(notRefSet);
            this.AutoGenerateNTSet.ExceptWith(notRefSet);

            foreach (var symbol in notRefSet) this.keyManager.Remove(symbol);
        }

        /// <summary>
        /// Delete the not referenced symbols with auto-generated
        /// </summary>
        private void DelNotRefAGSymbolsCore()
        {
            HashSet<NonTerminal> refSet = new HashSet<NonTerminal>();

            foreach (var item in this.NonTerminalMultiples) refSet.UnionWith(item.ToNonTerminalSet());
            var notRefSet = this.AutoGenerateNTSet.Except(refSet).ToHashSet();

            this.NonTerminalMultiples.ExceptWith(notRefSet);
            this.AutoGenerateNTSet.ExceptWith(notRefSet);

            foreach (var symbol in notRefSet) this.keyManager.Remove(symbol);
        }

        /// <summary>
        /// Delete the not referenced symbols with auto-generated
        /// </summary>
        private void DelNotRefAGSymbols()
        {
            while (true)
            {
                var prevCnt = this.AutoGenerateNTSet.Count;
                this.DelNotRefAGSymbolsCore();

                if (prevCnt == this.AutoGenerateNTSet.Count) break;
            }
        }

        public void Optimization()
        {
//            Optimizer.EliminateNeedlessAGNode(this.NonTerminalMultiples);
            this.DelNotRefAGSymbols();
        }

        public NonTerminal CreateVirtualSymbolForLRParsing(string newSymbolName)
        {
            NonTerminal newSymbol = new NonTerminal(newSymbolName, true);
            this.keyManager.AllocateUniqueKey(newSymbol);
            newSymbol.Add(this.StartSymbol);

            this.NonTerminalMultiples.Add(newSymbol);

            return newSymbol;
        }

        public Terminal GetTerminal(string value) => this.TerminalSet.ContainFirst(value);

        public NonTerminal CreateAutoGeneratedNT()
        {
            string name = "G";

            for (int i = 1; i < 100000; i++)
            {
                bool findUniqueName = true;
                name += i.ToString();

                foreach (var nonTerminal in this.NonTerminalMultiples)
                {
                    if (nonTerminal.Name == name)
                    {
                        findUniqueName = false;
                        break;
                    }
                }

                if (findUniqueName) break;
            }

            var result = new NonTerminal(name, false, true);
            keyManager.AllocateUniqueKey(result);

            this.NonTerminalMultiples.Add(result);
            this.AutoGenerateNTSet.Add(result);

            return result;
        }


        public override string ToString() => this.GetType().Name;
    }


    public class ScopeInfo
    {
        public Terminal StartTerminal { get; }
        public Terminal EndTerminal { get; }

        public ScopeInfo(Terminal startTerminal, Terminal endTerminal)
        {
            this.StartTerminal = startTerminal;
            this.EndTerminal = endTerminal;
        }

        public override string ToString() => string.Format("{0},{1}", this.StartTerminal, this.EndTerminal);
    }
}
