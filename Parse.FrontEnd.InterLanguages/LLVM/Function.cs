using Parse.FrontEnd.InterLanguages.Datas;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.InterLanguages.LLVM
{
    public class Function : List<IRUnit>, IRUnit
    {
        private string _comment;
        private IRFuncData _funcData;

        public string Label;

        public string Comment => ";" + _comment;

        public Function(IRFuncData funcData)
        {
            _funcData = funcData;
        }

        public Function(IRFuncData funcData, IRBlock block) : this(funcData)
        {
            this.AddRange(block);
        }

        public string ToFormatString()
        {
            string result = string.Format("define {0} @{1}(", _funcData.ReturnType, _funcData.Name);

            foreach (var varData in _funcData.Arguments)
                result += varData.Type + ",";

            if (_funcData.Arguments.Count > 0) result = result.Substring(0, result.Length - 1);
            result += ")";

            result += "{" + Environment.NewLine;
            foreach(var format in this)
                result += format.ToFormatString() + Environment.NewLine;
            result += "}";

            return result;
        }

        public override string ToString() => ToFormatString();
    }
}
