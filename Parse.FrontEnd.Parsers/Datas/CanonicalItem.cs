using Parse.RegularGrammar;
using System;

namespace Parse.FrontEnd.Parsers.Datas
{
    public class CanonicalItem : ICloneable
    {
        private sbyte markIndex = 0;

        public NonTerminalSingle singleNT { get; }
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
                if (this.singleNT.IsEpsilon) return true;

                return (this.markIndex >= this.singleNT.Count);
            }
        }

        public CanonicalItem(NonTerminalSingle singleNT)
        {
            this.singleNT = singleNT;
        }

        public CanonicalItem(NonTerminalSingle singleNT, sbyte markIdx)
        {
            this.singleNT = singleNT;
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

            if (this.markIndex-index < this.singleNT.Count)
            {
                result = this.singleNT[this.markIndex-index];
            }

            return result;
        }

        public override string ToString()
        {
            string result = this.singleNT.Name + " ->";

            for (int i = 0; i < this.singleNT.Count; i++)
            {
                result += (i == this.markIndex) ? "." : " ";

                if(this.singleNT[i] != new Epsilon())
                    result += this.singleNT[i].ToString();
            }

            if (this.markIndex >= this.singleNT.Count) result += ".";

            return result;
        }

        public bool Equals(CanonicalItem other)
        {
            if (object.ReferenceEquals(other, null)) return false;

            return (this.GetHashCode() == other.GetHashCode());
        }

        public override int GetHashCode() => System.Convert.ToInt32(this.singleNT.GetHashCode().ToString() + this.markIndex.ToString());

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

        public object Clone() => new CanonicalItem(this.singleNT.Clone() as NonTerminalSingle, this.markIndex);
    }
}
