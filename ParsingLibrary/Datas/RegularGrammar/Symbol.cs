using ParsingLibrary.Utilities;
using System;

namespace ParsingLibrary.Datas.RegularGrammar
{
    public abstract class Symbol : IShowable
    {
        internal UInt32 uniqueKey = 0xffffffff;

        public abstract string ToGrammarString();
        public abstract string ToTreeString(ushort depth = 1);

        public Symbol()
        {
        }

        public bool Equals(Symbol other)
        {
            if (object.ReferenceEquals(other, null)) return false;

            return (this.GetHashCode() == other.GetHashCode());
        }

        public override int GetHashCode() => (int)this.uniqueKey;

        public override bool Equals(object obj)
        {
            bool result = false;

            if (obj is Symbol)
            {
                Symbol right = obj as Symbol;

                result = (this.GetHashCode() == right.GetHashCode());
            }

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
        /// https://www.lucidchart.com/documents/edit/d05e9c87-a3ab-4b64-8a75-c6b84c28aa45/0
        /// https://www.lucidchart.com/documents/edit/d05e9c87-a3ab-4b64-8a75-c6b84c28aa45/1
        /// https://www.lucidchart.com/documents/edit/d05e9c87-a3ab-4b64-8a75-c6b84c28aa45/2
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static NonTerminal operator +(Symbol left, Symbol right)
        {
            NonTerminal result = AutoGenerator.GetNonTerminal();

            result.AddAsConcat(left, right);
            return result;
        }

        /// <summary>
        /// https://www.lucidchart.com/documents/edit/d05e9c87-a3ab-4b64-8a75-c6b84c28aa45/0
        /// https://www.lucidchart.com/documents/edit/d05e9c87-a3ab-4b64-8a75-c6b84c28aa45/1
        /// https://www.lucidchart.com/documents/edit/d05e9c87-a3ab-4b64-8a75-c6b84c28aa45/2
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static NonTerminal operator |(Symbol left, Symbol right)
        {
            NonTerminal result = AutoGenerator.GetNonTerminal();

            result.AddAsAlter(left, right);
            return result;
        }
    }
}
