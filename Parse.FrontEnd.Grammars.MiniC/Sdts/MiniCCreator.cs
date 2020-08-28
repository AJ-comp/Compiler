using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables;
using Parse.Types;
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
                                                                            ExprNode value)
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
                                                                varProperty, value);
            }
            else if (typeDatas.DataType == MiniCDataType.Int)
            {
                result = new IntVariableMiniC(typeDatas,
                                                            nameToken,
                                                            levelToken,
                                                            dimensionToken,
                                                            blockLevel,
                                                            offset,
                                                            varProperty,
                                                            value);
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
