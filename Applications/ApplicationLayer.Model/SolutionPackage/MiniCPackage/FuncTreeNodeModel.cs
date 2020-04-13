using ApplicationLayer.Common;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using System;
using System.Collections.Generic;

namespace ApplicationLayer.Models.SolutionPackage.MiniCPackage
{
    public class FuncTreeNodeModel : TreeNodeModel
    {
        public List<VarTreeNodeModel> Params { get; set; } = new List<VarTreeNodeModel>();

        public DataType ReturnType { get; set; } = DataType.Unknown;

        public string FuncName { get; set; } = string.Empty;

        public override string DisplayName
        {
            get
            {
                string result = FuncName + "(";
                foreach(var param in Params)
                {
                    result += param.DataType + ",";
                }
                if (Params.Count > 0) result = result.Substring(0, result.Length - 1);
                result += ")";

                if (ReturnType != DataType.Void) result += " : " + ReturnType;

                return result;
            }
            set => throw new Exception();
        }

        public override string FullOnlyPath => string.Empty;

        public override event EventHandler<FileChangedEventArgs> Changed;

        public override void RemoveChild(TreeNodeModel nodeToRemove)
        {
        }

    }
}
