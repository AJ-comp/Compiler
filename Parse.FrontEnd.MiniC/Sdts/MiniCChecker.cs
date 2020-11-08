using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables;
using System;
using System.Threading.Tasks;

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

            Parallel.Invoke(
                () =>
                {
                    findLogicInVarTable();
                });

            return result;
        }
    }
}
