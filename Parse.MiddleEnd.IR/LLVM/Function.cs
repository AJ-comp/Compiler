using Parse.MiddleEnd.IR.Datas;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parse.MiddleEnd.IR.LLVM
{
    public class Function : List<IRUnit>, IRUnit
    {
        private string _comment = string.Empty;
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
                result += varData.TypeName + ",";

            if (_funcData.Arguments.Count() > 0) result = result[0..^1];
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
