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
            if (var.InitialExpr == null) result += Helper.GetEnumDescription(State.NotInit);
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
                else result += Helper.GetEnumDescription(var.InitialExpr.Result.ValueState);
            }

            result += string.Format(" [Block: {0}, Offset: {1}]",
                                                var.Block,
                                                var.Offset);

            return result;
        }
    }
}
