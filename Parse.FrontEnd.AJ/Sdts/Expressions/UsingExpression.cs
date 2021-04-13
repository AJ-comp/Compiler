using Parse.FrontEnd.AJ.Sdts.Datas;
using System;

namespace Parse.FrontEnd.AJ.Sdts.Expressions
{
    /// ****************************************/
    /// <summary>
    /// using 문을 의미하는 표현식입니다.
    /// ex : using Name
    /// </summary>
    /// ****************************************/
    public class UsingExpression : AJExpression
    {
        public string Name { get; }

        public UsingExpression(IUsingExpression expression)
        {
            Name = expression.Name;
        }

        public override string ToString()
            => string.Format("using {0};", Name) + Environment.NewLine;
    }
}
