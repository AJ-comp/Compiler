using Parse.FrontEnd.Ast;
using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts
{
    public partial class MiniCSdts
    {
        private object CommonExpressionLogic(AstNonTerminal curNode)
        {
            List<AstSymbol> result = new List<AstSymbol>();

            if (curNode[2] is AstNonTerminal)
                result.AddRange((curNode[2] as AstNonTerminal).ActionLogic() as IReadOnlyList<AstSymbol>);
            else result.Add(curNode[2]);

            if (curNode[0] is AstNonTerminal)
                result.AddRange((curNode[0] as AstNonTerminal).ActionLogic() as IReadOnlyList<AstSymbol>);
            else result.Add(curNode[0]);

            result.Add(curNode);

            return result;
        }

        private object BuildOnlyAssignExpression(AstNonTerminal curNode)
        {
            List<AstSymbol> result = new List<AstSymbol>();

            // leftExp is always TreeTerminal so it has to only check rightExp.
            if (curNode[2] is AstNonTerminal)
                result.AddRange((curNode[2] as AstNonTerminal).ActionLogic() as IReadOnlyList<AstSymbol>);
            else 
                result.Add(curNode[2]);

            result.Add(curNode);

            return result;
        }

        private object ActionAdd(AstNonTerminal curNode) => CommonExpressionLogic(curNode);

        private object ActionSub(AstNonTerminal curNode) => CommonExpressionLogic(curNode);

        private object ActionMul(AstNonTerminal curNode) => CommonExpressionLogic(curNode);

        private object ActionDiv(AstNonTerminal curNode) => CommonExpressionLogic(curNode);

        private object ActionMod(AstNonTerminal curNode) => CommonExpressionLogic(curNode);

        private object ActionAssign(AstNonTerminal curNode) => BuildOnlyAssignExpression(curNode);

        private object ActionAddAssign(AstNonTerminal curNode) => CommonExpressionLogic(curNode);

        private object ActionSubAssign(AstNonTerminal curNode) => CommonExpressionLogic(curNode);

        private object ActionMulAssign(AstNonTerminal curNode) => CommonExpressionLogic(curNode);

        private object ActionDivAssign(AstNonTerminal curNode) => CommonExpressionLogic(curNode);

        private object ActionModAssign(AstNonTerminal curNode) => CommonExpressionLogic(curNode);

        private object ActionLogicalOr(AstNonTerminal curNode) => CommonExpressionLogic(curNode);

        private object ActionLogicalAnd(AstNonTerminal curNode) => CommonExpressionLogic(curNode);

        private object ActionLogicalNot(AstNonTerminal curNode) => CommonExpressionLogic(curNode);

        private object ActionEqual(AstNonTerminal curNode) => CommonExpressionLogic(curNode);

        private object ActionNotEqual(AstNonTerminal curNode) => CommonExpressionLogic(curNode);

        private object ActionGreaterThan(AstNonTerminal curNode) => CommonExpressionLogic(curNode);

        private object ActionLessThan(AstNonTerminal curNode) => CommonExpressionLogic(curNode);

        private object ActionGreatherEqual(AstNonTerminal curNode) => CommonExpressionLogic(curNode);

        private object ActionLessEqual(AstNonTerminal curNode) => CommonExpressionLogic(curNode);

        private object ActionUnaryMinus(AstNonTerminal curNode)
        {
            return null;
        }

        private object ActionPreInc(AstNonTerminal curNode)
        {
            return null;
        }

        private object ActionPreDec(AstNonTerminal curNode)
        {
            return null;
        }

        private object ActionPostInc(AstNonTerminal curNode)
        {
            return null;
        }

        private object ActionPostDec(AstNonTerminal curNode)
        {
            return null;
        }
    }
}
