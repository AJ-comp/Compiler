using ApplicationLayer.Common.Interfaces;

namespace ApplicationLayer.Models.SolutionPackage
{
    public class ErrorProjectTreeNodeModel : ProjectTreeNodeModel, IManagedable
    {
        public override string ProjectType => "error";

        public override bool IsChanged => false;

        public ErrorProjectTreeNodeModel(string path, string projName) : base(path, projName)
        {
            this.IsEditable = false;
        }

        public override void LoadElement()
        {
        }

        public override void RemoveChild(TreeNodeModel nodeToRemove)
        {
        }

        public override void Save()
        {
        }

        public override void SyncWithCurrentValue()
        {
        }

        public override void SyncWithLoadValue()
        {
        }
    }
}
