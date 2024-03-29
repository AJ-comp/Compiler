﻿using ApplicationLayer.Common;
using Parse.FrontEnd.AJ.Data;
using System;

namespace ApplicationLayer.Models.SolutionPackage.MiniCPackage
{
    public class VarTreeNodeModel : TreeNodeModel
    {
        public bool IsConst => (_varData != null) && _varData.Type.Const;
        public string DataType => (_varData != null) ? _varData.DataType.ToString() : string.Empty;
        public string Name => (_varData != null) ? _varData.Name : string.Empty;
        public string Dimension => (_varData != null) ? _varData.Dimension.ToString() : string.Empty;
        public string BlockIndex => (_varData != null) ? _varData.Block.ToString() : string.Empty;
        public string Offset => (_varData != null) ? _varData.Offset.ToString() : string.Empty;
        public bool IsGlobal => (_varData?.Block== 1);

        public override string DisplayName
        {
            get => $"{Name} : {DataType}";
            set => throw new Exception();
        }

        public override string FullOnlyPath => string.Empty;

        public override event EventHandler<FileChangedEventArgs> Changed;

        public VarTreeNodeModel(VariableAJ varData)
        {
            _varData = varData;
        }

        public override void RemoveChild(TreeNodeModel nodeToRemove)
        {
        }


        private VariableAJ _varData;
    }
}
