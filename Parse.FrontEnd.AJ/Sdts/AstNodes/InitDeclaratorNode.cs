﻿using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public class InitDeclaratorNode : AJNode
    {
        public InitDeclaratorNode(AstSymbol node) : base(node)
        {
        }

//        public int Level => System.Convert.ToInt32(LevelToken?.Input);
//        public string VarName => NameToken?.Input;

        public IEnumerable<TokenData> ArrayTokens => LeftVar.ArrayTokens;
        public TokenData NameToken => LeftVar.NameToken;

        public DeclareVarNode LeftVar { get; private set; }
        public ExprNode Right { get; private set; }


        /**************************************************/
        /// <summary>
        /// It starts semantic analysis for initialize expression.
        /// 초기화 식에 대한 의미분석을 시작합니다.
        /// format summary
        /// [0] : VariableNode   [Variable]
        /// [1] : LiteralNode?    [LiteralNode]
        ///      | UseVarNode?   [UseVar]
        ///      | ExprNode?
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        /**************************************************/
        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            // build Simple or ArrayVar
            LeftVar = Items[0].Compile(param) as DeclareVarNode;

            if (Items.Count > 1)
                Right = Items[1].Compile(param) as ExprNode;

            return this;
        }
    }
}
