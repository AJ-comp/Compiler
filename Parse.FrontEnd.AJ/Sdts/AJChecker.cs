using Parse.FrontEnd.AJ.Sdts.AstNodes;
using Parse.FrontEnd.AJ.Sdts.Datas.Variables;
using System;
using System.Threading.Tasks;

namespace Parse.FrontEnd.AJ.Sdts
{
    public class AJChecker
    {
        public static bool IsAllArithmetic(MiniCDataType left, MiniCDataType right)
        {
            return (left == MiniCDataType.Int && right == MiniCDataType.Int);
        }

        public static bool IsDefinedSymbol(AJNode node, TokenData token)
        {
            bool result = false;
            var variable = node.GetSymbol(token);

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

        public static bool CanAddVarData(AJNode node, VariableMiniC varData)
        {
            bool result = true;

            Action findLogicInVarTable = new Action(() =>
            {
                var varNodeList = node.GetSymbols(varData.NameToken);

                Parallel.ForEach(varNodeList, (record) =>
                {
                    if (record.Name != varData.Name) return;
                    if (record.Block != varData.Block) return;

                    result = false;
                    // duplicated declaration (in local block)
                    node.AddDuplicatedError(varData.NameToken);
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
