using Parse.MiddleEnd.IR.Interfaces;
using System.ComponentModel;

namespace Parse.MiddleEnd.IR.Datas
{
    public enum IRKeyword
    {
        [Description("this")] This
    }

    public interface IRType
    {
        string Name { get; }
    }

    public interface IRDeclareVar : IRType, IRItem, IHasDclVarProperties
    {
        IRExpr InitialExpr { get; set; }

        public bool IsMember => (PartyName.Length > 0);
    }

    public interface IRDeclareUserTypeVar : IRDeclareVar
    {
        string TypeName { get; }
    }

    public interface IRDeclareStructTypeVar : IRDeclareUserTypeVar
    {
        IRStructDefInfo DefInfo { get; }
        uint BiggestMemberSize { get; }
    }

    public interface IRSignableVar : IRDeclareVar
    {
        public bool Signed { get; }
    }

    public interface IRDoubleVar : IRDeclareVar
    {
        public bool Nan { get; }
    }
}
