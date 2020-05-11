﻿using ApplicationLayer.Common;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using System;
using System.Collections.Generic;

namespace ApplicationLayer.Models.SolutionPackage.MiniCPackage
{
    public class FuncTreeNodeModel : TreeNodeModel
    {
        private FuncData _funcData;

        public string ReturnType => (_funcData == null) ? string.Empty : _funcData.DclSpecData.DataType.ToString();
        public string Name => _funcData.Name;
        public string Offset => _funcData.Offset.ToString();
        public bool CanReference { get; } = true;

        public override string DisplayName
        {
            get
            {
                string result = Name + "(";
                foreach(var param in _funcData.ParamVars)
                {
                    result += param.DclData.DclSpecData.DataType + ",";
                }
                if (_funcData.ParamVars.Count > 0) result = result.Substring(0, result.Length - 1);
                result += ")";

                if (ReturnType != DataType.Void.ToString()) result += " : " + ReturnType.ToString();

                return result;
            }
            set => throw new Exception();
        }

        public override string FullOnlyPath => string.Empty;

        public override event EventHandler<FileChangedEventArgs> Changed;

        public FuncTreeNodeModel(FuncData funcData, bool canReference = true)
        {
            _funcData = funcData;
            CanReference = canReference;
        }

        public override void RemoveChild(TreeNodeModel nodeToRemove)
        {
        }

    }
}
