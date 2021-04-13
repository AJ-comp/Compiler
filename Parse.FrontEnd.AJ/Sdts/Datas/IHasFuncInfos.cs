using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Sdts.Datas
{
    public interface IHasFuncInfos
    {
        IEnumerable<FuncDefData> FuncList { get; }
    }
}
