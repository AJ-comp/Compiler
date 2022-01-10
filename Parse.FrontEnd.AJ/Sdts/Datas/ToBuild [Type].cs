using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Sdts.Datas.Variables;
using Parse.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts.Datas
{
    /*********************************************/
    /// <summary>
    /// 타입 구조 정의에 대한 인터페이스 입니다.         <br/>
    /// </summary>
    /*********************************************/
    public interface ITypeBuildNode : IBuildNode, IHasName
    {
        AJDataType Type { get; }
        uint Size { get; }
    }


    /*********************************************/
    /// <summary>
    /// user-defined 타입 구조 정의에 대한 인터페이스 입니다.
    /// </summary>
    /*********************************************/
    public interface IUserDefTypeBuildNode : ITypeBuildNode
    {
    }


    /*********************************************/
    /// <summary>
    /// 열거자 타입 구조 정의에 대한 인터페이스 입니다.
    /// </summary>
    /*********************************************/
    public interface IEnumTypeBuildNode : IUserDefTypeBuildNode
    {

    }
}
