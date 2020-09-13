using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables;
using Parse.FrontEnd.Grammars.Properties;
using System;
using System.Threading.Tasks;
using static Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables.VariableMiniC;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts
{
    public class MiniCChecker
    {
        public static bool IsAllArithmetic(MiniCDataType left, MiniCDataType right)
        {
            return (left == MiniCDataType.Int && right == MiniCDataType.Int);
        }


        public static bool CanAddVarData(MiniCNode node, VariableMiniC varData)
        {
            bool result = true;

            Action findLogicInVarTable = new Action(() =>
            {
                Parallel.ForEach(node.SymbolTable.VarTable, (record) =>
                {
                    if (record.DefineField.Name != varData.Name) return;

                    result = false;
                    // duplicated declaration (in local block)
                    MiniCUtilities.AddDuplicatedError(node, varData.NameToken);
                });
            });

            Action findLogicInDefineTable = new Action(() =>
            {
                foreach (var symbolTable in MiniCUtilities.GetReferableSymbolTablelList(node))
                {
                    Parallel.ForEach(symbolTable.DefineTable, (record) =>
                    {
                        if (record.DefineField.Name != varData.Name) return;

                        result = false;
                        // duplicated declaration (in define block)
                        MiniCUtilities.AddErrorUseDefinedIdent(node, varData.NameToken);
                    });
                }
            });

            Parallel.Invoke(
                () =>
                {
                    findLogicInDefineTable();
                },
                () =>
                {
                    findLogicInVarTable();
                });

            return result;
        }
    }
}
