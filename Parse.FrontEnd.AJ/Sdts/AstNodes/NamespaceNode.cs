using Parse.Extensions;
using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public class NamespaceNode : AJNode
    {
        public TokenDataList NameTokens { get; } = new TokenDataList();


        public int Block { get; private set; }
        public List<AJNode> References { get; } = new List<AJNode>();
        public string FullName => NameTokens.ToListString();


        public NamespaceNode(AstSymbol node) : base(node)
        {
        }


        /**************************************************/
        /// <summary>
        /// <para>Start semantic analysis for namespace.</para>
        /// <para>namespace에 대한 의미분석을 시작합니다.</para>
        /// [0:n] : Ident chain (TerminalNode List)  <br/>
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        /**************************************************/
        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            base.CompileLogic(param);

            foreach (var item in Items)
            {
                // parsing for ident chain
                var nameNode = item.Compile(param) as DefNameNode;
                NameTokens.AddExceptNull(nameNode.Token);
            }

            References.Add(this);
            //            DBContext.Instance.Insert(this);

            return this;
        }


        public bool IsContaindSymbol(TokenDataList defineSymbol)
        {
            if (NameTokens.Count > defineSymbol.Count) return false;

            for (int i = 0; i < NameTokens.Count; i++)
            {
                if (NameTokens[i].Input != defineSymbol[i].Input) return false;
            }

            return true;
        }

        public IEnumerable<string> GetEffictiveCases()
        {
            List<string> result = new List<string>();

            string accumlateString = string.Empty;
            result.Add(accumlateString);

            for (int i = NameTokens.Count - 1; i == 0; i--)
            {
                accumlateString += (accumlateString.Length > 0)
                                        ? accumlateString += "." + NameTokens[i].Input
                                        : NameTokens[i].Input;

                result.Add(accumlateString);
            }

            return result;
        }
    }
}
