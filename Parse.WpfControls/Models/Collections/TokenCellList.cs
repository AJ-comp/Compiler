using System.Collections.Generic;
using System.Threading.Tasks;
//using System.Collections.Generic;

namespace Parse.WpfControls.Models.Collections
{
    public class TokenCellList
    {
        public List<TokenCell> cells = new List<TokenCell>();


        private void Sort()
        {
            this.cells.Sort(delegate (TokenCell t, TokenCell td)
            {
                return (t.StartIndex < td.StartIndex) ? -1 : (t.StartIndex > td.StartIndex) ? 1 : 0;
            });
        }

        private void UpdateStartIndex(int sIndex, int value)
        {
            if (sIndex >= this.cells.Count) return;

            Parallel.For(sIndex, this.cells.Count, i =>
            {
                this.cells[i].StartIndex += value;
            });
        }

        /// <summary>
        /// This function returns the first index that the start index is >= the caret index.
        /// </summary>
        /// <param name="caretIndex"></param>
        /// <returns></returns>
        public int Find(int caretIndex)
        {
            return -1;
        }

        public void Add(TokenCell tokenCell)
        {
            if (tokenCell == null) return;

//            if(tokenCell.)
            this.Sort();
        }

        public void RemoveAt(int cellIndex)
        {
            if (cellIndex >= this.cells.Count) return;

            this.cells.RemoveAt(cellIndex);
            this.Sort();
        }

        public void RemoveRange(int sCellIndex, int len)
        {
            for(int i=0; i<len; i++)
            {
                if (sCellIndex >= this.cells.Count) break;

                this.cells.RemoveAt(sCellIndex);
            }


        }

        public void UpdateAllStartIndexes()
        {
            int sum = 0;

            for (int i = 0; i < this.cells.Count; i++)
            {
                this.cells[i].StartIndex += sum;
                sum += this.cells[i].Data.Length;
            }
        }

        /// <summary>
        /// This function returns a token index that contains a caret index.
        /// </summary>
        /// <param name="caretIndex">The caret index.</param>
        /// <param name="recognitionWay">The way that recognizes the caret index in the token index.</param>
        /// <returns>The token index that contains a caret index.</returns>
        public int Contains(int caretIndex, RecognitionWay recognitionWay)
        {
            int result = -1;

            Parallel.For(0, this.cells.Count, (i, loopOption) =>
            {
                var tokenCell = this.cells[i];

                if(tokenCell.Contains(caretIndex, recognitionWay))
                {
                    result = i;
                    loopOption.Stop();
                }
            });

            return result;
        }

        public void Merge(int sCellIndex, int len)
        {
            if (len <= 0) return;
            int eCellIndex = sCellIndex + len;

            string sumString = string.Empty;
            for (int i = sCellIndex; i <= eCellIndex; i++)
            {
                if (i >= this.cells.Count) break;
                sumString += this.cells[i].Data;
            }

            var cell = this.cells[sCellIndex];

            this.Replace(sCellIndex, new TokenCell(cell.StartIndex, sumString, TokenPatternInfo.NotDefinedToken));
            this.RemoveRange(sCellIndex + 1, len);
        }


        /// <summary>
        /// This function replaces the token of tokenIndex(argument) to the cell(argument).
        /// </summary>
        /// <param name="cellIndex"></param>
        /// <param name="cell"></param>
        public void Replace(int cellIndex, TokenCell cell)
        {
            if (cell == null) return;
            if (cellIndex >= this.cells.Count) return;

            this.cells[cellIndex] = cell;
        }
    }
}
