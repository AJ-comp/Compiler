﻿using System;

namespace Parse.FrontEnd.RegularGrammar
{
    public abstract class Symbol : IShowable, IQuantifiable, IConvertableEbnfString
    {
        protected string EbnfString { get; set; }
        public UInt32 UniqueKey { get; internal set; } = UInt32.MaxValue;

        public abstract string ToEbnfString(bool bContainLHS = false);
        public abstract string ToGrammarString();
        public abstract string ToTreeString(ushort depth = 1);


        protected Symbol()
        {
        }

        public bool Equals(Symbol other)
        {
            if (object.ReferenceEquals(other, null)) return false;

            return this.GetHashCode() == other.GetHashCode();
        }

        public override int GetHashCode() => (int)this.UniqueKey;

        public override bool Equals(object obj)
        {
            bool result = false;

            if (obj is Symbol)
            {
                Symbol right = obj as Symbol;

                result = this.GetHashCode() == right.GetHashCode();
            }

            return result;
        }

        public NonTerminal ZeroOrMore()
        {
            if (AutoGeneratedNTStore.ZeroOrMoreList.ContainsKey(this))
                return AutoGeneratedNTStore.ZeroOrMoreList[this];

            NonTerminal result = AutoGenerator.GetNonTerminal();
            NonTerminal bridge = AutoGenerator.GetNonTerminal();

            result.AddAsAlter(bridge, new Epsilon());
            bridge.AddAsConcat(this, result);

            AutoGeneratedNTStore.ZeroOrMoreList.Add(this, result);

            result.EbnfString = $"({this.ToEbnfString()})*";
            return result;
        }

        public NonTerminal OneOrMore()
        {
            if (AutoGeneratedNTStore.OneOrMoreList.ContainsKey(this))
                return AutoGeneratedNTStore.OneOrMoreList[this];

            NonTerminal result = AutoGenerator.GetNonTerminal();
            NonTerminal bridge = AutoGenerator.GetNonTerminal();

            result.AddAsConcat(this, bridge);
            bridge.AddAsAlter(result, new Epsilon());

            AutoGeneratedNTStore.OneOrMoreList.Add(this, result);

            result.EbnfString = $"({this.ToEbnfString()})+";
            return result;
        }

        public NonTerminal Optional()
        {
            if (AutoGeneratedNTStore.OptionList.ContainsKey(this))
                return AutoGeneratedNTStore.OptionList[this];

            NonTerminal result = AutoGenerator.GetNonTerminal();
            result.AddAsAlter(this, new Epsilon());

            AutoGeneratedNTStore.OptionList.Add(this, result);

            result.EbnfString = $"({this.ToEbnfString()})?";
            return result;
        }

        public static bool operator ==(Symbol left, Symbol right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(Symbol left, Symbol right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return !object.ReferenceEquals(right, null);
            }

            return !left.Equals(right);
        }

        /// <summary>
        /// Convert to the NonTerminal type that included concatenation information of left symbol and right symbol.
        /// </summary>
        /// <param name="left">left symbol</param>
        /// <param name="right">right symbol</param>
        /// <returns></returns>
        /// <see cref="https://www.lucidchart.com/documents/edit/d05e9c87-a3ab-4b64-8a75-c6b84c28aa45/0"/>
        /// <see cref="https://www.lucidchart.com/documents/edit/d05e9c87-a3ab-4b64-8a75-c6b84c28aa45/1"/>
        /// <see cref="https://www.lucidchart.com/documents/edit/d05e9c87-a3ab-4b64-8a75-c6b84c28aa45/2"/>
        public static NonTerminal operator +(Symbol left, Symbol right)
        {
            NonTerminal result = AutoGenerator.GetNonTerminal();

            result.AddAsConcat(left, right);
            result.EbnfString = $"{left.ToEbnfString()} {right.ToEbnfString()}";

            return result;
        }

        /// <summary>
        /// Convert to the NonTerminal type that included alternation information of left symbol and right symbol.
        /// </summary>
        /// <param name="left">left symbol</param>
        /// <param name="right">right symbol</param>
        /// <returns></returns>
        /// <see cref="https://www.lucidchart.com/documents/edit/d05e9c87-a3ab-4b64-8a75-c6b84c28aa45/0"/>
        /// <see cref="https://www.lucidchart.com/documents/edit/d05e9c87-a3ab-4b64-8a75-c6b84c28aa45/1"/>
        /// <see cref="https://www.lucidchart.com/documents/edit/d05e9c87-a3ab-4b64-8a75-c6b84c28aa45/2"/>
        public static NonTerminal operator |(Symbol left, Symbol right)
        {
            NonTerminal result = AutoGenerator.GetNonTerminal();

            result.AddAsAlter(left, right);
            result.EbnfString = $"{left.ToEbnfString()} | {right.ToEbnfString()}";

            return result;
        }
    }
}
