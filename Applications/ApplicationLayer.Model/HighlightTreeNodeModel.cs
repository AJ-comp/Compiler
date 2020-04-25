using ApplicationLayer.Common;
using ApplicationLayer.Common.Utilities;
using ApplicationLayer.Models.SolutionPackage;
using Parse.FrontEnd.DrawingSupport;
using System;
using System.Drawing;

namespace ApplicationLayer.Models
{
    public class HighlightTreeNodeModel : TreeNodeModel
    {
        public Brush ForegroundBrush;
        public Brush BackgroundBrush;

        public override string DisplayName { get; set; }

        public override string FullOnlyPath => string.Empty;

        public override event EventHandler<FileChangedEventArgs> Changed;

        public void Assign(ClassHierarchyData data)
        {
            DisplayName = HighlightMapItem.TokenTypeString(data.Data);

            foreach(var item in data.Items)
            {
                var newNode = new HighlightTreeNodeModel();
                newNode.Assign(item);

                children.Add(newNode);
            }
        }

        public override void RemoveChild(TreeNodeModel nodeToRemove)
        {
            throw new NotImplementedException();
        }
    }
}
