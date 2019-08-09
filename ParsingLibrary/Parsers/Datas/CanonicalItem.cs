using ParsingLibrary.Datas.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParsingLibrary.Parsers.Datas
{
    public class CanonicalItem : ICloneable
    {
        private sbyte markIndex = 0;

        public NonTerminalSingle singleNT { get; }
        public Symbol MarkSymbol
        {
            get
            {
                Symbol result = null;

                if (this.markIndex < this.singleNT.Count)
                {
                    result = this.singleNT[this.markIndex];
                    if (result == new Epsilon()) result = null;
                }

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

        public void MoveMarkerSymbol()
        {
            if (this.MarkSymbol != null) this.markIndex++;
        }

        public override string ToString()
        {
            string result = this.singleNT.Name + " -> ";

            for (int i = 0; i < this.singleNT.Count; i++)
            {
                if (i == this.markIndex) result += ".";

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

        public override int GetHashCode() => Convert.ToInt32(this.singleNT.GetHashCode().ToString() + this.markIndex.ToString());

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
