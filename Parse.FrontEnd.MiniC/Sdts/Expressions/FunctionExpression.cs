using Parse.Extensions;
using Parse.FrontEnd.MiniC.Sdts.Datas;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Interfaces;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.MiniC.Sdts.Expressions
{
    /// ***********************************************/
    /// <summary>
    /// 함수 정의를 의미하는 표현식입니다.
    /// ex : 
    /// ReturnType Name (ParamVars)
    /// {
    ///     Statement
    /// }
    /// </summary>
    /// ***********************************************/
    public class FunctionExpression : AJExpression, IConvertableToExpression<IRFuncDefInfo>
    {
        public IEnumerable<VariableExpression> ParamVars => _paramVars;
        public IRStatement Statement { get; }

        public FunctionExpression(IFunctionExpression funcExpression)
        {
            _data = funcExpression;

            // add this pointer type
            _paramVars.Add(VariableExpression.CreateThisReference(funcExpression.PartyInfo.Name, 0, 0, 0));

            foreach (var param in funcExpression.ParamVars)
            {
                if (param.TypeKind == Types.StdType.Struct || param.TypeKind == Types.StdType.Enum)
                    _paramVars.Add(new UserDefVarExpression(param));
                else
                    _paramVars.Add(new PreDefVarExpression(param));
            }

            Statement = StatementExpression.Create(funcExpression.Statement);
        }

        public override string ToString()
        {
            var result = string.Format("{0}{1} {2}", 
                                                    (_data.IsConstReturn) ? "const" : string.Empty,
                                                    Helper.GetEnumDescription(_data.ReturnType),
                                                    _data.Name);

            result += "(";
            result += ParamVars.ItemsString(PrintType.String);
            result += ")" + Environment.NewLine;

            result += _data.Statement.ToString() + Environment.NewLine;

            return result;
        }

        public IRFuncDefInfo ToIRData()
        {
            return new IRFuncDefInfo(ParamVars,
                                                  MiniCTypeConverter.ToStdDataType(_data.ReturnType),
                                                  _data.Name,
                                                  Statement);
        }

        private IFunctionExpression _data;
        private List<VariableExpression> _paramVars = new List<VariableExpression>();
    }
}
