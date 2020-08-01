using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables;
using Parse.Types.ConstantTypes;
using static Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables.VariableMiniC;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts
{
    public class MiniCCreator
    {
        public static VariableMiniC CreateVarData(MiniCTypeInfo typeDatas,
                                                                            TokenData nameToken,
                                                                            TokenData levelToken, 
                                                                            TokenData dimensionToken,
                                                                            int blockLevel, 
                                                                            int offset, 
                                                                            VarProperty varProperty, 
                                                                            object value)
        {
            VariableMiniC result = null;

            if (typeDatas.DataType == MiniCDataType.String)
            {
                result = new StringVariableMiniC(typeDatas,
                                                                nameToken,
                                                                levelToken,
                                                                dimensionToken,
                                                                blockLevel,
                                                                offset,
                                                                varProperty, new StringConstant(string.Empty));
            }
            else if (typeDatas.DataType == MiniCDataType.Int)
            {
                var initValue = (value == null) ? new IntConstant(0, Types.State.NotInit, 0)
                                                             : new IntConstant((int)value);

                result = new IntVariableMiniC(typeDatas,
                                                            nameToken,
                                                            levelToken,
                                                            dimensionToken,
                                                            blockLevel,
                                                            offset,
                                                            varProperty,
                                                            initValue);
            }

            return result;
        }


        public static VariableMiniC CreateVarData(MiniCTypeInfo typeDatas, 
                                                                            TokenData nameToken,
                                                                            TokenData levelToken, 
                                                                            TokenData dimensionToken,
                                                                            int blockLevel, 
                                                                            int offset,
                                                                            VarProperty varProperty)
        {
            return CreateVarData(typeDatas, nameToken, levelToken, dimensionToken, blockLevel, offset, varProperty, null);
        }
    }
}
