using ApplicationLayer.Common;
using ApplicationLayer.Models.SolutionPackage;
using System;

namespace ApplicationLayer.Models
{
    public class UCodeTreeNodeModel : TreeNodeModel
    {
        public string Label { get; set; }
        public string OpCode { get; set; }
        public string Operand1 { get; set; }
        public string Operand2 { get; set; }
        public string Operand3 { get; set; }
        public string Comment { get; set; }

        public override string DisplayName { get; set; }

        public override string FullOnlyPath => throw new NotImplementedException();

        public override event EventHandler<FileChangedEventArgs> Changed;

        public override void RemoveChild(TreeNodeModel nodeToRemove)
        {
            throw new NotImplementedException();
        }
    }
}
