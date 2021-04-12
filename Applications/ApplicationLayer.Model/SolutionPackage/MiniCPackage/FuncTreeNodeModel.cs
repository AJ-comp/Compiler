using ApplicationLayer.Common;
using Parse.FrontEnd.MiniC.Sdts.Datas;
using Parse.FrontEnd.MiniC.Sdts.Datas.Variables;
using System;
using System.Linq;

namespace ApplicationLayer.Models.SolutionPackage.MiniCPackage
{
    public class FuncTreeNodeModel : TreeNodeModel
    {
        private FuncDefData _funcData;

        public string ReturnType => (_funcData == null) ? string.Empty : _funcData.ReturnType.ToString();
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

                if (ReturnType != MiniCDataType.Void.ToString()) result += " : " + ReturnType.ToString();

                return result;
            }
            set => throw new Exception();
        }

        public override string FullOnlyPath => string.Empty;

        public override event EventHandler<FileChangedEventArgs> Changed;

        public FuncTreeNodeModel(FuncDefData funcData, bool canReference = true)
        {
            _funcData = funcData;
            CanReference = canReference;
        }

        public override void RemoveChild(TreeNodeModel nodeToRemove)
        {
        }

    }
}
