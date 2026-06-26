using System;

namespace Janglim
{
    public interface ICloneable<T> where T : class
    {
        T Clone();
    }
}
