using System;
using System.Collections.Generic;
using System.Text;

namespace AJ.Common
{
    public interface IExportable
    {
        object To();
    }

    public interface IExportable<T> where T : class
    {
        T To();
        T To(T from);
    }
}
