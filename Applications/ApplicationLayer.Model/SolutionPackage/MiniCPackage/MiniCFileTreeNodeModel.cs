using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Models.SolutionPackage.MiniCPackage
{
    public class MiniCFileTreeNodeModel : FileTreeNodeModel
    {
        public ObservableCollection<VarTreeNodeModel> Vars = new ObservableCollection<VarTreeNodeModel>();
        public ObservableCollection<FuncTreeNodeModel> Funcs = new ObservableCollection<FuncTreeNodeModel>();

        public string Data { get; set; }

        public MiniCFileTreeNodeModel(string path, string fileName) : base(path, fileName)
        {
            Vars.Add(new VarTreeNodeModel(DataType.Int, "test"));

            List<VarTreeNodeModel> paramList = new List<VarTreeNodeModel>();
            paramList.Add(new VarTreeNodeModel(DataType.Int, "param1"));
            paramList.Add(new VarTreeNodeModel(DataType.Int, "param2"));
            Funcs.Add(new FuncTreeNodeModel() { Params = paramList, ReturnType = DataType.Int, FuncName = "func" });
        }
    }
}
