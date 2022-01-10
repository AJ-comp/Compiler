using AJ.Common.Helpers;
using Parse.FrontEnd.Properties;
using Parse.FrontEnd.Types.Operations;
using Parse.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.Types
{
    public abstract class PLType
    {
        public abstract StdType TypeKind { get; }
        public abstract int Size { get; }
        public int PointerDepth { get; }


        public MeaningErrInfo GetErrorWithTP0001(PLType operand, string operSymbol)
        {
            return new MeaningErrInfo(nameof(AlarmCodes.TP0001),
                                                    string.Format(AlarmCodes.TP0001, TypeKind.ToDescription(), operSymbol, operand.TypeKind.ToDescription()));
        }

        public (PLType, MeaningErrInfoList) ArithmeticCompare(PLType operand, string operSymbol)
        {
            var errList = new MeaningErrInfoList();

            if (!(operand is IArithmeticOperation))
            {
                errList.Add(GetErrorWithTP0001(operand, operSymbol));
                return (null, errList);
            }

            return (new BitType(), errList);
        }

        public (PLType, MeaningErrInfoList) ArithmeticOperation(PLType operand, string operatorSymbol)
        {
            var errList = new MeaningErrInfoList();
            if (operand is IArithmeticOperation) return (GreaterType(operand), errList);

            errList.Add(GetErrorWithTP0001(operand, operatorSymbol));
            return (null, errList);
        }


        public PLType GreaterType(PLType operand)
        {
            if (!(operand is IArithmeticOperation)) return null;

            return (Size >= operand.Size) ? this : operand;
        }
    }
}
