using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public class DeclareIdentNode : AJNode
    {
        public AJTypeInfo AJType { get; protected set; }
        public TokenData NameToken { get; protected set; }

        public DeclareIdentNode(AstSymbol node) : base(node)
        {
        }


        /**************************************************/
        /// <summary>
        /// <para>Start semantic analysis for variable declaration.</para>
        /// <para>변수 선언에 대한 의미분석을 시작합니다.</para>
        /// format summary  <br/>
        /// [0] : Const? (AstTerminal)  <br/>
        /// [1] : typespecifier <br/>
        /// [2] : ident  (AstTerminal)  <br/>
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        /**************************************************/
        public override SdtsNode Compile(CompileParameter param)
        {
            bool bConst = false;
            base.Compile(param);
            var firstNode = Items[0].Compile(param);

            // const
            if (firstNode is TerminalNode) bConst = true;
            else _typeNode = firstNode as TypeDeclareNode;

            var secondNode = Items[1].Compile(param);
            if (secondNode is TypeDeclareNode)
            {
                _typeNode = secondNode as TypeDeclareNode;
                _ident = Items[2].Compile(param) as DefNameNode;
            }
            else _ident = secondNode as DefNameNode;

            AJType = _typeNode.ToAJTypeInfo(bConst);
            NameToken = _ident.Token;

            return this;
        }


        private TypeDeclareNode _typeNode;
        private DefNameNode _ident;
    }
}
