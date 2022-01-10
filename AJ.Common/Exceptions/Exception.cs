using System;
using System.Collections.Generic;
using System.Text;

namespace AJ.Common.Exceptions
{
    public class Exception<T> : Exception
    {
        public new T Data { get; }

        public Exception(T data)
        {
            Data = data;
        }
    }
}
