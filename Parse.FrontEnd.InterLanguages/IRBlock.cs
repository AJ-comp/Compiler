using Parse.MiddleEnd.IR.LLVM;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR
{
    /// <summary>
    /// Null value can't be added to the IRBlock
    /// IRBlock Implement the IList to implement the above feature.
    /// </summary>
    public class IRBlock : IList<IRUnit>, IRUnit
    {
        private string _comment;
        private List<IRUnit> _data = new List<IRUnit>();

        public string Label { get; }
        public string Comment => (_comment.Length > 0) ? "; " + _comment : string.Empty;

        public int Count => _data.Count;
        public bool IsReadOnly => false;

        public IRUnit this[int index] { get => _data[index]; set => _data[index] = value; }

        public IRBlock()
        {
        }

        public IRBlock(string label, string comment = "")
        {
            Label = label;
            _comment = comment;
        }

        public string ToFormatString()
        {
            string result = Comment + Environment.NewLine;
            result += Label + Environment.NewLine;

            foreach (var item in this)
                result += item + Environment.NewLine;

            return result;
        }

        public int IndexOf(IRUnit item) => _data.IndexOf(item);

        public void Insert(int index, IRUnit item) => _data.Insert(index, item);

        public void RemoveAt(int index) => _data.RemoveAt(index);

        public void Add(IRUnit item)
        {
            if (item != null) _data.Add(item);
        }

        public void AddRange(IEnumerable<IRUnit> items)
        {
            foreach (var item in items) Add(item);
        }

        public void Clear() => _data.Clear();

        public bool Contains(IRUnit item) => _data.Contains(item);

        public void CopyTo(IRUnit[] array, int arrayIndex) => _data.CopyTo(array, arrayIndex);

        public bool Remove(IRUnit item) => _data.Remove(item);

        public IEnumerator<IRUnit> GetEnumerator() => ((IList<IRUnit>)_data).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IList<IRUnit>)_data).GetEnumerator();
    }
}
