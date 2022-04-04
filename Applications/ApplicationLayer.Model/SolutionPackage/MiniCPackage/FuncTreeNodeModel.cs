using ApplicationLayer.Common;
using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Sdts.AstNodes;
using System;
using System.Linq;

namespace ApplicationLayer.Models.SolutionPackage.MiniCPackage
{
    public class FuncTreeNodeModel : TreeNodeModel
    {
        private FuncDefNode _funcData;

        public string ReturnType => (_funcData == null) ? string.Empty : _funcData.ReturnDataType.ToString();
        public string Name => _funcData.Name;
        public string Offset => _funcData.Offset.ToString();
        public bool IsGlobal => true;
        public bool CanReference { get; } = true;

        public override string DisplayName
        {
            get
            {
                string result = Name + "(";
                var paramVars = _funcData.ParamVarList;

                foreach (var param in paramVars)
                {
                    result += param.DataType + ",";
                }
                if (paramVars.Count() > 0) result = result.Substring(0, result.Length - 1);
                result += ")";

                if (ReturnType != AJDataType.Void.ToString()) result += " : " + ReturnType.ToString();

                return result;
            }
            set => throw new Exception();
        }

        public override string FullOnlyPath => string.Empty;

        public override event EventHandler<FileChangedEventArgs> Changed;

        public FuncTreeNodeModel(FuncDefNode funcData, bool canReference = true)
        {
            _funcData = funcData;
            CanReference = canReference;
        }

        public override void RemoveChild(TreeNodeModel nodeToRemove)
        {
        }

    }
}
