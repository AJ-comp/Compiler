using ApplicationLayer.Common;
using ApplicationLayer.Common.Utilities;
using ApplicationLayer.Models.SolutionPackage;
using Parse.FrontEnd.Support.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ApplicationLayer.Models
{
    public class HighlightTreeNodeModel : TreeNodeModel
    {
        public Type Type { get; private set; }
        public Color ForegroundColor { get; set; }
        public Color BackgroundColor { get; set; }

        public Color ValidForegroundColor
        {
            get
            {
                if (this.ForegroundColor != Color.Transparent) return this.ForegroundColor;
                if (this.Parent == null) return Color.Transparent;

                return (this.Parent as HighlightTreeNodeModel).ValidForegroundColor;
            }
        }

        public Color ValidBackgroundColor
        {
            get
            {
                if (this.BackgroundColor != Color.Transparent) return this.BackgroundColor;
                if (this.Parent == null) return Color.Transparent;

                return (this.Parent as HighlightTreeNodeModel).ValidBackgroundColor;
            }
        }

        public IReadOnlyList<HighlightTreeNodeModel> ToList
        {
            get
            {
                List<HighlightTreeNodeModel> result = new List<HighlightTreeNodeModel>();

                foreach(var item in this._children)
                {
                    var convertItem = item as HighlightTreeNodeModel;

                    result.AddRange(convertItem.ToList);
                }

                result.Add(this);
                return result;
            }
        }

        public override string DisplayName
        {
            get => HighlightMapHelper.Instance.TokenTypeString(Type);
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

        public void AssignDefaultValue()
        {
            this.ForegroundColor = HighlightMapHelper.Instance.DefaultForegroundColor(this.Type);
            this.BackgroundColor = HighlightMapHelper.Instance.DefaultBackgroundColor(this.Type);

            foreach (var item in this._children)
                (item as HighlightTreeNodeModel).AssignDefaultValue();
        }

        public override void RemoveChild(TreeNodeModel nodeToRemove)
        {
            throw new NotImplementedException();
        }
    }
}
