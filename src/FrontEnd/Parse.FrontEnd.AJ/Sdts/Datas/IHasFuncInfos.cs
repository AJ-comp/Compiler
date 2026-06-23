using Parse.FrontEnd.AJ.Sdts.AstNodes;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Sdts.Datas
{
    public interface IHasFuncInfos
    {
        IEnumerable<FuncDefNode> FuncList { get; }
    }
}
