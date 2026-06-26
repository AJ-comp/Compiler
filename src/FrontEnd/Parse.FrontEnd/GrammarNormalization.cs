using Janglim.FrontEnd.RegularGrammar;
using System.Collections.Generic;
using System.Linq;

namespace Janglim.FrontEnd
{
    /// <summary>
    /// Grammar normalization: rewrites a fully-defined grammar into an equivalent, cleaner one
    /// before its parse table is built. (Replaces the former <c>Optimizer</c>.)
    ///
    /// <para><b>Flattening / needless-node removal</b> (<see cref="OptSingleChildNode(NonTerminal)"/>,
    /// <see cref="OptConcatNode"/>, <see cref="OptAltNode"/>, <see cref="EliminateNeedlessAGNode"/>):
    /// folds away the auto-generated intermediate nodes that the EBNF operators
    /// (<c>+</c>, <c>|</c>, <c>?</c>, <c>*</c>, <c>+</c>) create when they are redundant.</para>
    ///
    /// <para><b>Optional-absorb</b> (<see cref="AbsorbOptionals"/>): expands <c>X?</c>
    /// (<c>G -&gt; X | epsilon</c>) sitting in front of other symbols into explicit alternatives,
    /// removing the epsilon that forces an early reduce (and the conflict it causes). MeaningUnit
    /// and Priority ride along — each rewritten production is a <see cref="NonTerminalConcat.Clone"/>
    /// of the original, so a production with k optionals expands to 2^k alternatives.</para>
    /// </summary>
    public static class GrammarNormalization
    {
        // ---------------------------------------------------------------------------------------
        //  Optional-absorb (epsilon elimination for `?`)
        // ---------------------------------------------------------------------------------------

        /// <summary>
        /// 옵션( <c>X?</c> )을 명시적인 두 갈래로 펼쳐서 ε(빈 가지)를 없앤다.
        ///
        /// 옵션 뒤에 다른 심볼이 따라오면 그 ε가 파서에게 "너무 이른 결정"을 강요해 충돌을 만든다.
        /// ε를 제거하면 그 충돌도 사라진다. MU/priority는 펼쳐진 두 규칙에 그대로 복사된다.
        ///
        /// <code>
        ///   before:  A -> G b       (G is auto-generated)
        ///            G -> const | ε
        ///
        ///   after:   A -> const b   (G removed)
        ///            A -> b
        /// </code>
        /// </summary>
        public static void AbsorbOptionals(IEnumerable<NonTerminal> nonTerminals, ISet<NonTerminal> optionalNodes)
        {
            if (optionalNodes.Count == 0) return;

            foreach (var nt in nonTerminals.ToList())
            {
                if (optionalNodes.Contains(nt)) continue;   // leave the optional nodes themselves alone

                var original = new List<NonTerminalConcat>();
                for (int i = 0; i < nt.Count; i++) original.Add(nt.ElementAt(i));

                if (!original.Any(c => ContainsOptional(c, optionalNodes))) continue;

                var expanded = new List<NonTerminalConcat>();
                foreach (var c in original) expanded.AddRange(ExpandOptionals(c, optionalNodes));

                nt.Clear();
                foreach (var c in expanded) nt.Add(c);
            }
        }

        private static bool ContainsOptional(NonTerminalConcat concat, ISet<NonTerminal> optionalNodes)
        {
            foreach (var symbol in concat)
                if (symbol is NonTerminal nt && optionalNodes.Contains(nt)) return true;

            return false;
        }

        /// <summary>
        /// 한 생성규칙 안의 옵션들을 재귀적으로 펼쳐 가능한 모든 조합을 만든다.
        /// 옵션 하나를 찾을 때마다 "있는 갈래 / 없는 갈래" 둘로 갈라 재귀한다 (옵션 k개 → 2^k 규칙).
        /// </summary>
        private static List<NonTerminalConcat> ExpandOptionals(NonTerminalConcat concat, ISet<NonTerminal> optionalNodes)
        {
            int pos = -1;
            NonTerminal opt = null;
            for (int i = 0; i < concat.Count; i++)
            {
                if (concat[i] is NonTerminal nt && optionalNodes.Contains(nt)) { pos = i; opt = nt; break; }
            }

            if (pos < 0) return new List<NonTerminalConcat> { concat };

            var nonEpsilon = NonEpsilonContent(opt);
            var result = new List<NonTerminalConcat>();

            // branch 1: optional present -> replace it with its non-epsilon content
            var present = concat.Clone();
            present.Replace(pos, nonEpsilon);
            result.AddRange(ExpandOptionals(present, optionalNodes));

            // branch 2: optional absent -> drop it
            var absent = concat.Clone();
            absent.RemoveAt(pos);
            result.AddRange(ExpandOptionals(absent, optionalNodes));

            return result;
        }

        private static NonTerminalConcat NonEpsilonContent(NonTerminal optional)
        {
            for (int i = 0; i < optional.Count; i++)
            {
                var c = optional.ElementAt(i);
                if (!c.IsEpsilon) return c;
            }

            return new NonTerminalConcat();
        }

        // ---------------------------------------------------------------------------------------
        //  Flattening / needless auto-generated node removal  (formerly Optimizer)
        // ---------------------------------------------------------------------------------------

        /// <summary>
        /// 문법 전체(모든 비단말)에 평탄화를 한 번 돌린다.
        /// EBNF 연산자( + | ? * + )가 만든 불필요한 "자동생성 중간 노드"를 접어 없앤다.
        ///
        /// 규칙이 다 정의된 뒤(테이블 빌드 전)에 한 번 호출한다.
        /// 비단말마다 SingleChild → Concat → Alt 순서로 적용하며, 각 규칙의 MU/priority는 보존된다.
        /// </summary>
        public static void FlattenAll(IEnumerable<NonTerminal> nonTerminals)
        {
            foreach (var nt in nonTerminals.ToList())
            {
                OptSingleChildNode(nt);
                OptConcatNode(nt);
                OptAltNode(nt);
            }
        }

        /// <summary>
        /// "대체(인라인) 가능한" 비단말들을 찾아 보고한다 — 자식이 단 하나의 NonTerminal인 경우.
        /// 진단(report)용일 뿐, 문법을 실제로 바꾸지는 않는다.
        /// </summary>
        public static ChangeableDataSet ChangeableNodeData(HashSet<NonTerminal> allNonTerminals)
        {
            ChangeableDataSet result = new ChangeableDataSet();

            foreach (var item in allNonTerminals)
            {
                // count of symbolList in the symbolListSet
                if (item.Count != 1) continue;

                // count of symbol in the symbolList
                var children = item.ElementAt(0);
                if (children.Count != 1) continue;

                Symbol symbol = children[0];
                if (symbol is Terminal) continue;

                NonTerminal child = (symbol as NonTerminal);
                if (child.IsStartSymbol) continue;

                NonTerminal key = result.ContainsElementOfValue(child);
                if (key != null) result[key].Add(item);
                else
                {
                    HashSet<NonTerminal> fromSet = new HashSet<NonTerminal>();
                    fromSet.Add(child);
                    result.Add(item, fromSet);
                }
            }

            return result;
        }

        /// <summary>
        /// 심볼이 "자동생성 비단말(G1, G2 …)"인지 판별한다.
        /// 자동생성 비단말 = 사용자가 직접 쓴 게 아니라 ? * + 와 +/| 연산자가 속으로 만든 중간 노드.
        /// </summary>
        private static bool IsAutoGenerated(Symbol src)
        {
            if (src is Terminal) return false;

            NonTerminal candidate = src as NonTerminal;
            if (!candidate.AutoGenerated) return false;

            return true;
        }

        /// <summary>
        /// 비단말의 생성규칙이 "자동생성 자식 하나"뿐일 때, 그 자식을 끌어와 풀어버린다 (1 depth).
        /// 이 버전은 그 자식이 문법 *다른 곳*에서도 쓰였다면 그 참조까지 전역으로 치환한다(AllReplace).
        ///
        /// <code>
        ///   before:  A -> G       (G is auto-generated)
        ///            G -> a | b
        ///
        ///   after:   A -> a | b   (G removed)
        /// </code>
        /// </summary>
        private static bool OptSingleChildNode(HashSet<NonTerminal> nonTerminals, NonTerminal target)
        {
            if (target.Count != 1) return false;

            NonTerminalSingle singleNT = target[0];

            if (singleNT.Count != 1) return false;
            if (singleNT[0] is Terminal) return false;

            var child = singleNT[0] as NonTerminal;
            if (!child.AutoGenerated) return false;

            target.SetChildrenOfItem(child);
            AllReplace(nonTerminals, child, target);

            return true;
        }

        /// <summary>
        /// 비단말의 생성규칙이 "자동생성 자식 하나"뿐일 때, 그 자식의 내용을 끌어와 풀어버린다 (지역 치환).
        /// 자식의 생성규칙들이 곧 이 비단말의 생성규칙이 되고, 원래 규칙의 MU/priority를 그 위에 복사한다.
        ///
        /// <code>
        ///   before:  A -> G     (G is auto-generated)
        ///            G -> x y
        ///
        ///   after:   A -> x y   (G removed)
        /// </code>
        /// </summary>
        public static bool OptSingleChildNode(NonTerminal target)
        {
            if (target.Count != 1) return false;

            NonTerminalSingle singleNT = target[0];

            if (singleNT.Count != 1) return false;
            if (singleNT[0] is Terminal) return false;

            var child = singleNT[0] as NonTerminal;
            if (!child.AutoGenerated) return false;

            // capture THIS production's priority/MU before the inlined ones overwrite the slot
            var priority = singleNT.Priority;
            var meaningUnit = singleNT.MeaningUnit;

            target.SetChildrenOfItem(child);
            target.Replace(child, target);   // Replace clones the inlined productions

            for (int i = 0; i < target.Count; i++)
            {
                target.ElementAt(i).Priority = priority;
                target.ElementAt(i).MeaningUnit = meaningUnit;
            }

            return true;
        }

        /// <summary>
        /// 생성규칙 안에 박힌 "자동생성 연결(concat) 노드"를 그 자리에 펼쳐 넣는다.
        /// 생성규칙의 *개수*는 그대로(1→1), 심볼만 한 줄로 펴진다. MU/priority 보존.
        ///
        /// <code>
        ///   before:  A -> G c     (G is auto-generated)
        ///            G -> a b
        ///
        ///   after:   A -> a b c   (G removed)
        /// </code>
        /// https://www.lucidchart.com/documents/edit/ee28348d-60e1-4c50-b6ad-3b3f434ff026/0
        /// </summary>
        public static bool OptConcatNode(NonTerminal target, HashSet<NonTerminal> explorationPreventSet = null)
        {
            if (explorationPreventSet == null) explorationPreventSet = new HashSet<NonTerminal>();
            explorationPreventSet.Add(target);

            NonTerminal temp = target.Clone() as NonTerminal;
            target.Clear();

            foreach (NonTerminalSingle singleNT in temp)
            {
                var addList = singleNT.ToNonTerminalConcat().Template() as NonTerminalConcat;

                foreach (var symbol in singleNT)
                {
                    if (!IsAutoGenerated(symbol)) { addList.Add(symbol); continue; }

                    NonTerminal child = symbol as NonTerminal;

                    // 이미 탐색한 노드라면
                    if (explorationPreventSet.Contains(child)) { addList.Add(child); continue; }

                    if (OptConcatNode(child, explorationPreventSet)) addList.AddRange(child.ElementAt(0));
                    else addList.Add(child);
                }

                target.Add(addList);
            }

            return (target.Count == 1);
        }

        /// <summary>
        /// 어떤 생성규칙이 "자동생성 선택(alt) 노드 하나"뿐이면, 그 갈래들을 부모 비단말로 끌어 올린다.
        /// 생성규칙이 1개 → 여러 개로 늘어난다. 끌어 올린 갈래마다 원래 규칙의 MU/priority를 복사한다.
        ///
        /// <code>
        ///   before:  A -> x | G       (G is auto-generated)
        ///            G -> a | b
        ///
        ///   after:   A -> x | a | b   (G removed)
        /// </code>
        /// https://www.lucidchart.com/documents/edit/ee28348d-60e1-4c50-b6ad-3b3f434ff026/1
        /// </summary>
        public static bool OptAltNode(NonTerminal target, HashSet<NonTerminal> explorationPreventSet = null)
        {
            if (explorationPreventSet == null) explorationPreventSet = new HashSet<NonTerminal>();
            explorationPreventSet.Add(target);

            NonTerminal temp = target.Clone() as NonTerminal;
            target.Clear();

            foreach (NonTerminalSingle singleNT in temp)
            {
                var addList = singleNT.ToNonTerminalConcat().Template() as NonTerminalConcat;

                bool firstRule = (singleNT.Count == 1);
                foreach (var symbol in singleNT)
                {
                    if (!IsAutoGenerated(symbol)) { addList.Add(symbol); continue; }

                    NonTerminal child = symbol as NonTerminal;

                    // 이미 탐색한 노드라면
                    if (explorationPreventSet.Contains(child)) { addList.Add(child); continue; }

                    bool secondRule = OptAltNode(child, explorationPreventSet);
                    if (firstRule && secondRule)
                    {
                        // inline the child's alternatives, carrying THIS production's priority/MU
                        for (int i = 0; i < child.Count; i++)
                        {
                            var inlined = child.ElementAt(i).Clone();
                            inlined.Priority = singleNT.Priority;
                            inlined.MeaningUnit = singleNT.MeaningUnit;
                            target.Add(inlined);
                        }
                    }
                    else addList.Add(child);
                }

                if (addList.Count > 0) target.Add(addList);
            }

            return (target.Count > 1);
        }

        /// <summary>
        /// 모든 비단말에 위 세 가지를 순서대로(SingleChild → Concat → Alt) 돌려 불필요한 자동생성 노드를 제거한다.
        /// 여기 쓰인 SingleChild 는 전역 치환(AllReplace) 버전이다.
        /// (per-rule 지역 버전을 일괄 적용하는 건 <see cref="FlattenAll"/> 참고)
        ///
        /// <code>
        ///   before:  A -> G     (G is auto-generated)
        ///            G -> a b
        ///
        ///   after:   A -> a b   (G removed)
        /// </code>
        /// </summary>
        public static void EliminateNeedlessAGNode(HashSet<NonTerminal> nonTerminals)
        {
            foreach (var target in nonTerminals)
            {
                OptSingleChildNode(nonTerminals, target);
                OptConcatNode(target);
                OptAltNode(target);
            }
        }

        /// <summary>
        /// 문법 전체를 훑어 <paramref name="from"/> 참조를 모두 <paramref name="to"/>로 바꾼다.
        /// </summary>
        public static void AllReplace(HashSet<NonTerminal> allNonTerminal, NonTerminal from, NonTerminal to)
        {
            foreach (var nonTerminal in allNonTerminal) nonTerminal.Replace(from, to);
        }
    }
}
