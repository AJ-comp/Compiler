using Parse.FrontEnd.Ast;
using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.ParseTree
{
    public abstract class ParseTreeSymbol : IShowable
    {
        public ParseTreeSymbol Parent { get; set; } = null;

        public abstract IReadOnlyList<TokenData> AllTokens { get; }

        public abstract bool IsVirtual { get; }
        public abstract bool IsMeaning { get; }
        public abstract bool IsTerminal { get; }
        public abstract bool HasVirtualChild { get; }
        public abstract AstSymbol ToAst { get; }
        public abstract string AllInputDatas { get; }

        public override bool Equals(object obj)
        {
            var target = obj as ParseTreeSymbol;

            if (IsVirtual != target.IsVirtual) return false;
            if (IsMeaning != target.IsMeaning) return false;
            if (IsTerminal != target.IsTerminal) return false;

            bool result = true;
            // it doens't check the child tokens.
            /*
            if (HasVirtualChild != target.HasVirtualChild) return false;
            if (AllTokens.Count != target.AllTokens.Count) return false;

            for (int i = 0; i < AllTokens.Count; i++)
            {
                if (AllTokens[i] != target.AllTokens[i])
                {
                    result = false;
                    break;
                }
            }
            */

            return result;
        }

        public override int GetHashCode()
        {
            int adder = 0;
            foreach (var token in AllTokens) adder += token.GetHashCode();

            return adder + HashCode.Combine(IsVirtual, IsMeaning, IsTerminal, HasVirtualChild);
        }

        public static bool operator ==(ParseTreeSymbol left, ParseTreeSymbol right)
        {
            return EqualityComparer<ParseTreeSymbol>.Default.Equals(left, right);
        }

        public static bool operator !=(ParseTreeSymbol left, ParseTreeSymbol right)
        {
            return !(left == right);
        }


        /**********************************************************************/
        /// <summary>
        /// Equals 함수는 노드 그자체만 비교하고, 이 함수는 노드의 전체 연결성까지 모두 비교합니다.
        /// </summary>
        /// <param name="target">비교할 노드</param>
        /// <returns></returns>
        /**********************************************************************/
        public bool IsEqualAll(ParseTreeSymbol target)
        {
            if (!Equals(target)) return false;

            return Parent.IsEqualAll(target.Parent);
        }


        public abstract string ToGrammarString();
        public abstract string ToTreeString(ushort depth = 1);
    }
}
