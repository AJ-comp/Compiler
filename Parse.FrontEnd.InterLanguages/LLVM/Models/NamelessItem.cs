﻿namespace Parse.FrontEnd.InterLanguages.LLVM.Models
{
    public class NamelessItem : TerminalItem
    {
        public object Value { get; protected set; }

        public NamelessItem(DataType type, object value) : base(type)
        {
            Value = value;
        }
    }
}
