using Parse.FrontEnd.MiniC.Sdts.Datas;
using Parse.FrontEnd.MiniC.Sdts.Expressions.ExprExpressions;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Interfaces;
using Parse.Types;

namespace Parse.FrontEnd.MiniC.Sdts.Expressions
{
    public abstract class VariableExpression : AJExpression, IRDeclareVar
    {
        public StdType TypeKind { get; }

        public string PartyName { get; }
        public int Block { get; set; }
        public int Offset { get; set; }

        public int Length { get; }

        public uint PointerLevel { get; set; }
        public IRExpr InitialExpr { get; set; }

        public string Name { get; }


        protected VariableExpression(IDeclareVarExpression var)
        {
            TypeKind = var.TypeKind;
            PartyName = var.PartyName;
            Block = var.Block;
            Offset = var.Offset;
            Length = var.Length;
            PointerLevel = var.PointerLevel;

            if (var.InitialExpr is DefaultExprData)
            {
                var cExpr = var.InitialExpr as DefaultExprData;
                InitialExpr = LiteralExpression.Create(cExpr.Result);
            }
            else InitialExpr = ExprExpression.Create(var.InitialExpr);
            Name = var.Name;
        }

        protected VariableExpression(StdType typeKind, string partyName, int length, string name)
        {
            TypeKind = typeKind;
            PartyName = partyName;
            Length = length;
            Name = name;
        }


        /// ***************************************************************/
        /// <summary>
        /// This 참조자를 생성합니다.
        /// </summary>
        /// <param name="partyName"></param>
        /// <param name="block"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        /// ***************************************************************/
        public static VariableExpression CreateThisReference(string partyName, int block, int offset, int length)
        {
            var result = new ThisVarExpression(StdType.Struct,
                                                                      partyName,
                                                                      length,
                                                                      Helper.GetEnumDescription(IRKeyword.This))
            {
                Block = block,
                Offset = offset,
                PointerLevel = 1
            };

            return result;
        }


        public static VariableExpression Create(IDeclareVarExpression varExpression)
        {
            if (string.IsNullOrEmpty(varExpression.TypeName)) return new PreDefVarExpression(varExpression);
            
            return new UserDefVarExpression(varExpression);
        }
    }
}
