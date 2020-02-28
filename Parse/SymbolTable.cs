using System;

namespace Parse.FrontEnd
{
    public abstract class SymbolTable : ICloneable
    {
        public abstract object Clone();

        /// <summary>
        /// This function merges this with target.
        /// If a collision property exist then this property value is used.
        /// A collision condition is in case this property value is not initial value and this property value is not equals with target property value.
        /// </summary>
        /// <param name="target">The target item to merge</param>
        /// <returns>The SymbolTable that merged</returns>
        public abstract SymbolTable Merge(SymbolTable target);
    }
}