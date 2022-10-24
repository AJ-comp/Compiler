﻿using Parse.Extensions;
using Parse.MiddleEnd.IR.Datas;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Parse.MiddleEnd.IR.Expressions
{
    public abstract class IRExpression
    {
        public int Id { get; } = Interlocked.Decrement(ref _nextId);
        public DebuggingInfo DebuggingInfo { get; } = new DebuggingInfo();
        public IRExpression Parent { get; }
        public List<IRExpression> Items { get; } = new List<IRExpression>();
        public bool AssignPosition { get; private set; } = false;


        public void SetAssignPosition(bool value)
        {
            foreach (var item in Items)
                item.SetAssignPosition(value);
        }

        public override string ToString()
        {
            string result = string.Empty;

            if (Items.Count > 0)
            {
                result += $", Expression: {GetType().Name} -> ";
                result += Items.ItemsString(PrintType.Type);
            }

            return result;
        }


        private static int _nextId = int.MaxValue;
        protected List<ITable> _tables = new List<ITable>();

        protected IRExpression()
        {
        }
    }
}
