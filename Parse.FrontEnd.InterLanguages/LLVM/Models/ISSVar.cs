using Parse.FrontEnd.InterLanguages.Datas.Types;
using Parse.MiddleEnd.IR.Datas;

namespace Parse.MiddleEnd.IR.LLVM.Models
{
    public interface ISSVar : ISSItem, IValue
    {
        DataType Type { get; }
        string Name { get; }
        //        int Block { get; }
        //        int Offset { get; }

//        int Length { get; }
    }
}
