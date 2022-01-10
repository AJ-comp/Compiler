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
        public AJTypeInfo AJType;
        public TokenData NameToken;

        public DeclareIdentNode(AstSymbol node) : base(node)
        {
        }


        /**************************************************/
        /// <summary>
        /// Start semantic analysis for variable declaration.
        /// 변수 선언에 대한 의미분석을 시작합니다.
        /// format summary
        /// [0] : Const? (AstTerminal)
        /// [1] : typespecifier 
        /// [2] : ident  (AstTerminal)
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        /**************************************************/
        public override SdtsNode Compile(CompileParameter param)
        {
            bool bConst = false;
            var firstNode = Items[0].Compile(param);

            // const
            if (firstNode is TerminalNode) bConst = true;
            else _typeNode = firstNode as TypeDeclareNode;

            var secondNode = Items[1].Compile(param);
            if (secondNode is TypeDeclareNode)
            {
                _typeNode = secondNode as TypeDeclareNode;
                _ident = Items[2].Compile(param) as TerminalNode;
            }
            else _ident = secondNode as TerminalNode;

            AJType = _typeNode.ToAJTypeInfo(bConst);
            NameToken = _ident.Token;

            return this;
        }


        private TypeDeclareNode _typeNode;
        private TerminalNode _ident;
    }
}
