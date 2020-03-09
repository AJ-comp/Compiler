using Parse.FrontEnd.RegularGrammar;
using System;

namespace Parse.FrontEnd.Parsers.Datas.LR
{
    public class CanonicalItem : ICloneable
    {
        private sbyte markIndex = 0;

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
        /// example A-> ab. does mark index reached the end?
        /// </summary>
        public bool IsEnd
        {
            get
            {
                if (this.SingleNT.IsEpsilon) return true;

                return (this.markIndex >= this.SingleNT.Count);
            }
        }

        public CanonicalItem(NonTerminalSingle singleNT)
        {
            this.SingleNT = singleNT;
        }

        public CanonicalItem(NonTerminalSingle singleNT, sbyte markIdx)
        {
            this.SingleNT = singleNT;
            this.markIndex = markIdx;
        }

        public void MoveMarkSymbol()
        {
            if (this.MarkSymbol != null) this.markIndex++;
        }

        /// <summary>
        /// This function gets the symbol as far away as an index from the markIndex.
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
                result += (i == this.markIndex) ? "." : " ";

                if(this.SingleNT[i] != new Epsilon())
                    result += this.SingleNT[i].ToString();
            }

            if (this.markIndex >= this.SingleNT.Count) result += ".";

            return result;
        }

        public bool Equals(CanonicalItem other)
        {
            if (object.ReferenceEquals(other, null)) return false;

            return (this.GetHashCode() == other.GetHashCode());
        }

        public override int GetHashCode() => System.Convert.ToInt32(this.SingleNT.GetHashCode().ToString() + this.markIndex.ToString());

        public override bool Equals(object obj)
        {
            bool result = false;

            if (obj is CanonicalItem)
            {
                CanonicalItem right = obj as CanonicalItem;

                result = (this.GetHashCode() == right.GetHashCode());
            }

            return result;
        }

        public static bool operator ==(CanonicalItem left, CanonicalItem right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(CanonicalItem left, CanonicalItem right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return !object.ReferenceEquals(right, null);
            }

            return !left.Equals(right);
        }

        public object Clone() => new CanonicalItem(this.SingleNT.Clone() as NonTerminalSingle, this.markIndex);
    }
}
