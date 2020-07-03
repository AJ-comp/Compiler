using System;

namespace Parse
{
    public interface ICloneable<T> where T : class
    {
        T Clone();
    }
}
