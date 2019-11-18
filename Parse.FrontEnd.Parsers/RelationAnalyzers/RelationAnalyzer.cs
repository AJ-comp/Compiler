using Parse.FrontEnd.Parsers;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.RelationAnalyzers;
using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;

namespace ParsingLibrary.Parsers.RelationAnalyzers
{
    internal class RelationAnalyzer
    {
        public FollowAnalyzer FollowAnalyzer { get; } = new FollowAnalyzer();
        public ParsingDic ParsingDic { get; } = new ParsingDic();

        public RelationAnalyzer()
        {
        }

        private void SingleFormulaAnalysis(NonTerminalSingle singleNT, NonTerminal item)
        {
            foreach (var terminal in Analyzer.FirstTerminalSet(singleNT))
            {
                if (terminal == new Epsilon())
                {
                    foreach (var followTerminal in this.FollowAnalyzer.Datas[item])
                    {
                        var key = new Tuple<Terminal, NonTerminal>(followTerminal, item);

                        if (this.ParsingDic.ContainsKey(key)) this.ParsingDic.Replace(key, singleNT);
                        else this.ParsingDic.Add(key, singleNT);
                    }
                }
                else
                {
                    var key = new Tuple<Terminal, NonTerminal>(terminal, item);

                    if (this.ParsingDic.ContainsKey(key)) this.ParsingDic.Replace(key, singleNT);
                    else this.ParsingDic.Add(key, singleNT);
                }
            }
        }

        public void Analysis(HashSet<NonTerminal> nonTerminals)
        {
            this.FollowAnalyzer.CalculateAllFollow(nonTerminals);

            foreach (var item in nonTerminals)
            {
                foreach(NonTerminalSingle singleNT in item)
                    this.SingleFormulaAnalysis(singleNT, item);
            }
        }
    }
}
