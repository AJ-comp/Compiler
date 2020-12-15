using Parse.FrontEnd.MiniC.Sdts.AstNodes;
using Parse.FrontEnd.MiniC.Sdts.Datas;
using Parse.FrontEnd.MiniC.Sdts.Datas.Variables;
using System;
using System.Threading.Tasks;

namespace Parse.FrontEnd.MiniC.Sdts
{
    public class MiniCChecker
    {
        public static bool IsAllArithmetic(MiniCDataType left, MiniCDataType right)
        {
            return (left == MiniCDataType.Int && right == MiniCDataType.Int);
        }

        public static bool IsDefinedVar(MiniCNode node, TokenData token)
        {
            bool result = false;
            var variable = MiniCUtilities.GetVarDCLSymbolData(node, token);

            if (variable == null)
            {
                result = false;
                node.ConnectedErrInfoList.Add(new MeaningErrInfo(nameof(Properties.AlarmCodes.MCL0001),
                                                                                            string.Format(Properties.AlarmCodes.MCL0001, token.Input)));
            }
            else
            {
                result = true;
                variable.ReferenceTable.Add(node);
            }

            return result;
        }

        public static bool CanAddVarData(MiniCNode node, VariableMiniC varData)
        {
            bool result = true;

            Action findLogicInVarTable = new Action(() =>
            {
                var varNodeList = MiniCUtilities.GetReferableVarDatas(node);

                Parallel.ForEach(varNodeList, (record) =>
                {
                    if (record.Name != varData.Name) return;

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
