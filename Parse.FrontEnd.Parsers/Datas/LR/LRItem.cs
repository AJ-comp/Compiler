using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.Parsers.Datas.LR
{
    public class LRItem : ICloneable
    {
        private sbyte markIndex = 0;

        public TerminalSet Follow { get; internal set; } = new TerminalSet();
        public TerminalSet LookAhead { get; internal set; } = new TerminalSet();


        public NonTerminalSingle SingleNT { get; }
        public Symbol MarkSymbol
        {
            get
            {
                Symbol result = this.GetSymbolFarMarkIndex(0);
                if (result == new Epsilon()) result = null;

                return result;
            }
        }

        /// <summary>
        /// The symbol immediately before the mark symbol.
        /// </summary>
        public Symbol PrevMarkSymbol
        {
            get
            {
                var result = this.GetSymbolFarMarkIndex(1);
                if (result == new Epsilon()) result = null;

                return result;
            }
        }

        public bool IsFirst => markIndex == 0;

        public NonTerminalConcat SymbolListAfterMarkSymbol => SingleNT.PostSymbolListFrom(markIndex);

        /// <summary>
        /// Get symbol list before mark symbol
        /// </summary>
        /// <example>
        /// in case A -> abc.B 
        /// get as [0]:a, [1]:b, [2]:c
        /// </example>
        public NonTerminalConcat SymbolListBeforeMarkSymbol => SingleNT.PrevSymbolListFrom(markIndex);


        /// <summary>
        /// example A-> ab. does mark index reached the end?
        /// </summary>
        public bool IsReachedHandle
        {
            get
            {
                if (this.SingleNT.IsEpsilon) return true;

                return this.markIndex >= this.SingleNT.Count;
            }
        }

        public LRItem(NonTerminalSingle singleNT)
        {
            this.SingleNT = singleNT;
        }

        public LRItem(NonTerminalSingle singleNT, sbyte markIdx)
        {
            this.SingleNT = singleNT;
            this.markIndex = markIdx;
        }


        public void MoveMarkSymbol()
        {
            if (this.MarkSymbol != null) this.markIndex++;
        }

        /// <summary>
        /// <para>Get the symbol as far away as an index from the markIndex.</para>
        /// <para>마킹 인덱스로부터 index 만큼 멀리 떨어진 symbol을 가져옵니다.</para>
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Symbol GetSymbolFarMarkIndex(int index)
        {
            Symbol result = null;
            if (this.markIndex-index < 0) return result;

            if (this.markIndex-index < this.SingleNT.Count)
            {
                result = this.SingleNT[this.markIndex-index];
            }

            return result;
        }

        public override string ToString()
        {
            string result = this.SingleNT.Name + " ->";

            for (int i = 0; i < this.SingleNT.Count; i++)
            {
                result += (i == this.markIndex) ? "•" : " ";

                if(this.SingleNT[i] != new Epsilon())
                    result += this.SingleNT[i].ToString();
            }

            if (this.markIndex >= this.SingleNT.Count) result += "•";

            return result;
        }

        public bool Equals(LRItem other)
        {
            if (object.ReferenceEquals(other, null)) return false;

            return (this.GetHashCode() == other.GetHashCode());
        }

        public override int GetHashCode() => System.Convert.ToInt32(this.SingleNT.GetHashCode().ToString() + this.markIndex.ToString());

        public override bool Equals(object obj)
        {
            bool result = false;

            if (obj is LRItem)
            {
                LRItem right = obj as LRItem;

                result = (this.GetHashCode() == right.GetHashCode());
            }

            return result;
        }

        public static bool operator ==(LRItem left, LRItem right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(LRItem left, LRItem right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return !object.ReferenceEquals(right, null);
            }

            return !left.Equals(right);
        }

        public LRItem FirstLRItem() => new LRItem(this.SingleNT.Clone() as NonTerminalSingle, 0);
        public LRItem PrevLRItem() => (markIndex <= 0) ? null : new LRItem(this.SingleNT.Clone() as NonTerminalSingle, (sbyte)(markIndex -1));
        public object Clone() => new LRItem(this.SingleNT.Clone() as NonTerminalSingle, markIndex);
    }
}
