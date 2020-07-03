using Parse.FrontEnd.InterLanguages.Datas.Types;
using System;

namespace Parse.MiddleEnd.IR.Datas
{
    public interface IRData
    {
        DataType Type { get; }
    }


    public interface IRData<out T> where T : DataType
    {
        bool IsNan { get; }
    }
}
