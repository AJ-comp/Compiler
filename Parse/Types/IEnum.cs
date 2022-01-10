using Parse.Types.ConstantTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.Types
{
    public interface IEnum : IDataTypeSpec
    {
        IConstant BitAnd(IConstant operand);
        IConstant BitOr(IConstant operand);
    }
}
