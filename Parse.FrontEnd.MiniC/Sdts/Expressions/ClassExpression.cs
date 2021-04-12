using Parse.FrontEnd.MiniC.Sdts.Datas;
using Parse.MiddleEnd.IR.Datas;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.MiniC.Sdts.Expressions
{
    /// ***********************************************/
    /// <summary>
    /// class 정의를 의미하는 표현식입니다.
    /// ex : 
    /// class Name
    /// {
    ///     IEnumerable<VariableExpression>
    ///     IEnumerable<FunctionExpression>
    /// }
    /// </summary>
    /// ***********************************************/
    public class ClassExpression : AJExpression, IConvertableToExpression<IRStructDefInfo>
    {
        public IEnumerable<VariableExpression> Vars => _vars;
        public IEnumerable<FunctionExpression> Funcs => _funcs;

        public ClassExpression(IClassExpression data)
        {
            _data = data;

            foreach (var field in data.VarList)
                _vars.Add(VariableExpression.Create(field));

            foreach (var func in data.FuncList)
                _funcs.Add(new FunctionExpression(func));
        }

        public IRStructDefInfo ToIRData()
        {
            List<IRFuncDefInfo> funcDefList = new List<IRFuncDefInfo>();
            foreach (var func in Funcs) funcDefList.Add(func.ToIRData());

            return new IRStructDefInfo(_data.Name, Vars, funcDefList);
        }

        public override string ToString()
        {
            var result = string.Format("class {0}", _data.Name);

            result += "{" + Environment.NewLine;

            foreach (var var in _vars)
                result += "\t" + var.ToString() + Environment.NewLine;

            foreach (var func in _funcs)
                result += "\t" + func.ToString() + Environment.NewLine;

            result += "}";

            return result;
        }



        private IClassExpression _data;
        private List<VariableExpression> _vars = new List<VariableExpression>();
        private List<FunctionExpression> _funcs = new List<FunctionExpression>();
    }
}
