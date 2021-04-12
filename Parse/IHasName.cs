using Parse.Types;

namespace Parse
{
    public interface IHasName
    {
        string Name { get; }
    }


    /// ********************************/
    /// <summary>
    /// 변수 선언에 필요한 정보를 부여합니다.
    /// </summary>
    /// ********************************/
    public interface IHasDclVarProperties
    {
        string PartyName { get; }
        StdType TypeKind { get; }
        int Block { get; set; }
        int Offset { get; set; }
        int Length { get; }
        uint PointerLevel { get; set; }
    }
}
