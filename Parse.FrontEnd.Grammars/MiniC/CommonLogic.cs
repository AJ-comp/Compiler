using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;
using Parse.MiddleEnd.IR;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Datas.Types;
using Parse.MiddleEnd.IR.Datas.ValueDatas;
using System;
using System.Collections.Generic;
using IR = Parse.MiddleEnd.IR.Datas.Types;

namespace Parse.FrontEnd.Grammars.MiniC
{
    public class CommonLogic
    {
        public static IReadOnlyList<TokenData> GetTokenDataList(IRData data)
        {
            List<TokenData> result = new List<TokenData>();

            if(data is MiniCVarData)
            {
                MiniCVarData cd = data as MiniCVarData;
                result.Add(cd.NameToken);
            }
            if (data is LiteralData)
            {
                LiteralData cd = data as LiteralData;
                result.Add(cd.ValueToken);
            }
            else if (data is ConceptValueData)
            {
                ConceptValueData cd = data as ConceptValueData;
                result.AddRange(cd.TokenList);
            }

            return result;
        }

        public static IRValue CommonOp(IRValue s, IRValue t, 
                                                        Func<double> doubleLogic, Func<int> intLogic,
                                                        Func<short> shortLogic, Func<byte> byteLogic,
                                                        Func<byte> bitLogic)
        {
            DType greaterType = IRChecker.GetGreaterType(s.TypeName, t.TypeName);

            List<TokenData> list = new List<TokenData>();
            list.AddRange(GetTokenDataList(s));
            list.AddRange(GetTokenDataList(t));

            if (greaterType == DType.Double) return new ConceptValueData<DoubleType>(list, doubleLogic.Invoke());
            else if (greaterType == DType.Int) return new ConceptValueData<Int>(list, intLogic.Invoke());
            else if (greaterType == DType.Short) return new ConceptValueData<Short>(list, shortLogic.Invoke());
            else if (greaterType == DType.Byte) return new ConceptValueData<IR.Byte>(list, byteLogic.Invoke());
            else if (greaterType == DType.Bit) return new ConceptValueData<Bit>(list, bitLogic.Invoke());

            return null;
        }
    }
}
