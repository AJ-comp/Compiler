using System.Collections.Generic;

namespace Parse.FrontEnd.MiniC.Sdts.Datas
{
    public interface IHasFuncInfos
    {
        IEnumerable<FuncData> FuncList { get; }
    }
}
