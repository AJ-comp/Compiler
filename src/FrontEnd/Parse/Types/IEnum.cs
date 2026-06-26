using Janglim.Types.ConstantTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Janglim.Types
{
    public interface IEnum : IDataTypeSpec
    {
        IConstant BitAnd(IConstant operand);
        IConstant BitOr(IConstant operand);
    }
}
