﻿using System;
using System.Collections.Generic;

namespace Parse.RegularGrammar
{
    public class NonTerminalSingle : NonTerminalConcat, IShowable
    {
        private sbyte alterIndex = 0;

        public UInt32 UniqueKey { get; } = 0;
        public bool IsStartSymbol { get; internal set; } = false;
        public bool AutoGenerated { get; } = false;
        public string Name { get; } = string.Empty;

        private NonTerminalSingle(string name, bool bStartSymbol = false, bool autoGenerated = false)
        {
            this.IsStartSymbol = bStartSymbol;
            this.Name = name;
            this.AutoGenerated = autoGenerated;
        }

        public NonTerminalSingle(NonTerminalSingle target) : base(target.Priority, target.MeaningUnit)
        {
            this.UniqueKey = target.UniqueKey;
            this.IsStartSymbol = target.IsStartSymbol;
            this.Name = target.Name;
            this.AutoGenerated = target.AutoGenerated;
            this.alterIndex = (sbyte)target.alterIndex;

            this.AddRange(target);
        }

        public NonTerminalSingle(NonTerminal target, int index, uint priority, Logic.MeaningUnit meaningUnit = Logic.MeaningUnit.Empty) : base(priority, meaningUnit)
        {
            this.UniqueKey = target.uniqueKey;
            this.IsStartSymbol = target.IsStartSymbol;
            this.Name = target.Name;
            this.AutoGenerated = target.AutoGenerated;
            this.alterIndex = (sbyte)index;

            if (index >= target.Count) return;
            this.AddRange(target.ElementAt(index));
        }

        public NonTerminal ToNonTerminal() => new NonTerminal(this);
        public NonTerminalConcat ToNonTerminalConcat()
        {
            var result = new NonTerminalConcat(this.Priority, this.symbols.ToArray());
            result.MeaningUnit = this.MeaningUnit;

            return result;
        }


        public string ToGrammarString()
        {
            string result = this.Name + " ->";

            foreach (var symbol in this)    result += " " + symbol.ToString();

            return result;
        }

        public string ToTreeString(ushort depth = 1)
        {
            string result = string.Empty;

            for (int i = 1; i < depth; i++) result += "  ";

            result += "Nonterminal : " + this.Name + Environment.NewLine;

            foreach (var symbol in this)
            {
                for (int i = 1; i < depth; i++) result += "  ";
                result += "  ";

                if (symbol is Terminal) result += "Terminal : " + symbol.ToString();
                else result += "NonTerminal : " + symbol.ToString();

                result += Environment.NewLine;
            }

            return result;
        }

        public override string ToString()
        {
            return this.ToGrammarString();
//            return this.Name;
        }

        public new object Clone() => new NonTerminalSingle(this);
        public new object Template()
        {
            NonTerminalSingle result = this.Clone() as NonTerminalSingle;
            result.Clear();

            return result;
        }

        public bool Equals(NonTerminalSingle other)
        {
            if (object.ReferenceEquals(other, null)) return false;

            return (this.GetHashCode() == other.GetHashCode());
        }

        public override int GetHashCode() => System.Convert.ToInt32(this.UniqueKey.ToString() + this.alterIndex.ToString());

        public override bool Equals(object obj)
        {
            bool result = false;

            if (obj is NonTerminalSingle)
            {
                NonTerminalSingle right = obj as NonTerminalSingle;

                result = (this.GetHashCode() == right.GetHashCode());
            }

            return result;
        }

        public static bool operator ==(NonTerminalSingle left, NonTerminalSingle right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(NonTerminalSingle left, NonTerminalSingle right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return !object.ReferenceEquals(right, null);
            }

            return !left.Equals(right);
        }
    }
}
