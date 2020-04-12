using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using System.Collections.Generic;

namespace ApplicationLayer.Models.SolutionPackage.MiniCPackage
{
    public class MiniCFileTreeNodeModel : FileTreeNodeModel
    {
        public MiniCFileTreeNodeModel(string path, string fileName) : base(path, fileName)
        {
            Children.Add(new VarTreeNodeModel(DataType.Int, "test"));

            List<VarTreeNodeModel> paramList = new List<VarTreeNodeModel>();
            paramList.Add(new VarTreeNodeModel(DataType.Int, "param1"));
            paramList.Add(new VarTreeNodeModel(DataType.Int, "param2"));
            Children.Add(new FuncTreeNodeModel() { Params = paramList, ReturnType = DataType.Int, FuncName = "func" });
        }
    }
}
