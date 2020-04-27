using ApplicationLayer.Common;
using ApplicationLayer.Common.Utilities;
using ApplicationLayer.Models.SolutionPackage;
using Parse.FrontEnd.DrawingSupport;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ApplicationLayer.Models
{
    public class HighlightTreeNodeModel : TreeNodeModel
    {
        public Type Type { get; set; }
        public Brush ForegroundBrush { get; }
        public Brush BackgroundBrush { get; }

        public Brush ValidForegroundBrush
        {
            get
            {
                if (this.ForegroundBrush != null) return this.ForegroundBrush;
                if (this.Parent == null) return null;

                return (this.Parent as HighlightTreeNodeModel).ValidForegroundBrush;
            }
        }

        public Brush ValidBackgroundBrush
        {
            get
            {
                if (this.BackgroundBrush != null) return this.BackgroundBrush;
                if (this.Parent == null) return null;

                return (this.Parent as HighlightTreeNodeModel).ValidBackgroundBrush;
            }
        }

        public IReadOnlyList<HighlightTreeNodeModel> ToList
        {
            get
            {
                List<HighlightTreeNodeModel> result = new List<HighlightTreeNodeModel>();

                foreach(var item in this.children)
                {
                    var convertItem = item as HighlightTreeNodeModel;

                    result.AddRange(convertItem.ToList);
                }

                return result;
            }
        }

        public override string DisplayName
        {
            get => HighlightMapItem.TokenTypeString(Type);
            set => throw new NotImplementedException();
        }

        public override string FullOnlyPath => string.Empty;

        public override event EventHandler<FileChangedEventArgs> Changed;

        public void Assign(ClassHierarchyData data)
        {
            Type = data.Data;

            foreach(var item in data.Items)
            {
                var newNode = new HighlightTreeNodeModel();
                newNode.Assign(item);

                this.AddChildren(newNode);
            }
        }

        public override void RemoveChild(TreeNodeModel nodeToRemove)
        {
            throw new NotImplementedException();
        }
    }
}
