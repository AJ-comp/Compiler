using Parse.FrontEnd.Ast;
using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts
{
    public partial class MiniCSdts
    {
        private object CommonCalculate(AstNonTerminal curNode, int blockLevel, int offset)
        {
            List<AstNonTerminal> result = new List<AstNonTerminal>();

            if (curNode.SignPost.MeaningUnit == this.Add)
                result.AddRange(ActionAdd(curNode, blockLevel, offset) as List<AstNonTerminal>);
            else if (curNode.SignPost.MeaningUnit == this.Sub)
                result.AddRange(ActionSub(curNode, blockLevel, offset) as List<AstNonTerminal>);
            else if (curNode.SignPost.MeaningUnit == this.Mul)
                result.AddRange(ActionMul(curNode, blockLevel, offset) as List<AstNonTerminal>);
            else if (curNode.SignPost.MeaningUnit == this.Div)
                result.AddRange(ActionDiv(curNode, blockLevel, offset) as List<AstNonTerminal>);
            else if (curNode.SignPost.MeaningUnit == this.Mod)
                result.AddRange(ActionMod(curNode, blockLevel, offset) as List<AstNonTerminal>);

            return result;
        }

        private object ActionAdd(AstNonTerminal curNode, int blockLevel, int offset)
        {
            List<AstNonTerminal> result = new List<AstNonTerminal>();

            // if TreeNonTerminal doesn't exist.
            if (curNode[0] is AstTerminal && curNode[2] is AstTerminal)
            {
                result.Add(curNode);
                return result;
            }

            // if at least one TreeNonTerminal exist.
            if (curNode[0] is AstNonTerminal)
                result.AddRange(CommonCalculate(curNode[0] as AstNonTerminal, blockLevel, offset) as List<AstNonTerminal>);
            else result.Add(curNode);

            if (curNode[2] is AstNonTerminal)
                result.AddRange(CommonCalculate(curNode[2] as AstNonTerminal, blockLevel, offset) as List<AstNonTerminal>);
            else result.Add(curNode);

            return result;
        }

        private object ActionSub(AstNonTerminal curNode, int blockLevel, int offset) => curNode;

        private object ActionMul(AstNonTerminal curNode, int blockLevel, int offset) => curNode;

        private object ActionDiv(AstNonTerminal curNode, int blockLevel, int offset) => curNode;

        private object ActionMod(AstNonTerminal curNode, int blockLevel, int offset) => curNode;

        private object ActionAssign(AstNonTerminal curNode, int blockLevel, int offset)
        {
            List<AstNonTerminal> result = new List<AstNonTerminal>();

            // leftExp is always TreeTerminal so it has to only check rightExp.
            if (curNode[2] is AstNonTerminal)
            {
                result.AddRange(CommonCalculate(curNode[2] as AstNonTerminal, blockLevel, offset) as List<AstNonTerminal>);
                result.Add(curNode);
            }
            else result.Add(curNode);

            return result;
        }

        private object ActionAddAssign(AstNonTerminal curNode, int blockLevel, int offset) => curNode;

        private object ActionSubAssign(AstNonTerminal curNode, int blockLevel, int offset) => curNode;

        private object ActionMulAssign(AstNonTerminal curNode, int blockLevel, int offset) => curNode;

        private object ActionDivAssign(AstNonTerminal curNode, int blockLevel, int offset) => curNode;

        private object ActionModAssign(AstNonTerminal curNode, int blockLevel, int offset) => curNode;

        private object ActionLogicalOr(AstNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionLogicalAnd(AstNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionLogicalNot(AstNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionEqual(AstNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionNotEqual(AstNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionGreaterThan(AstNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionLessThan(AstNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionGreatherEqual(AstNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionLessEqual(AstNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionUnaryMinus(AstNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionPreInc(AstNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionPreDec(AstNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionPostInc(AstNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }

        private object ActionPostDec(AstNonTerminal node, int blockLevel, int offset)
        {
            return null;
        }
    }
}
