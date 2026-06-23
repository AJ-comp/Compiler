using Parse.Types.ConstantTypes;

namespace Parse.FrontEnd.AJ.Sdts.Datas
{
    public interface IBuildNode
    {
    }

    public enum FuncType
    {
        Normal,
        Creator,
        Destructor
    }


    #region 식에 대한 인터페이스
    /*****************************/
    /// <summary>
    /// 식 기능을 위한 인터페이스 입니다.
    /// </summary>
    /*****************************/
    public interface IExprBuildNode : IBuildNode
    {
        IConstant Result { get; }
    }

    #endregion
}
