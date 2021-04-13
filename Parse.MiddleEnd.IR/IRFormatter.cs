using Parse.Extensions;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Interfaces;
using Parse.Types;

namespace Parse.MiddleEnd.IR
{
    public class IRFormatter
    {
        public static string ToDebugFormat(IRDeclareVar var)
        {
            string result = Helper.GetDescription(var.TypeKind) + " ";
            if (var is IRDeclareUserTypeVar)
            {
                var cVar = var as IRDeclareUserTypeVar;
                result += cVar.TypeName + " ";
            }

            for (int i = 0; i < var.PointerLevel; i++) result += "*";
            result += " " + var.Name;
            if (var.Length > 0) result += "[" + var.Length + "]" + " ";

            result += " = ";
            if (var.InitialExpr == null) result += State.NotInit.ToDescription();
            else
            {
                if (var.InitialExpr is IRUseVarExpr)
                {
                    var cVar = var.InitialExpr as IRUseVarExpr;
                    result += cVar.DeclareInfo.Name;
                }
                else if (var.InitialExpr.Result.ValueState == State.Fixed)
                {
                    var value = var.InitialExpr.Result.Value;
                    result += (value == null) ? "null" : value.ToString();
                }
                else result += var.InitialExpr.Result.ValueState.ToDescription();
            }

            result += $" [Block: {var.Block}, Offset: {var.Offset}]";

            return result;
        }
    }
}
