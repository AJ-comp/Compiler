using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public class DeclareVarNode : AJNode
    {
        public string VarName => NameToken?.Input;
        public int Dimension => (DimensionToken != null) ? System.Convert.ToInt32(DimensionToken?.Input) : 0;

        public TokenData NameToken { get; private set; }
        public TokenData DimensionToken { get; private set; }

        public DeclareVarNode(AstSymbol node) : base(node)
        {
        }


        /**************************************************/
        /// <summary>
        /// It starts semantic analysis for variable declaration.
        /// 변수 선언에 대한 의미분석을 시작합니다.
        /// format summary
        /// [0] : ident (AstTerminal)
        /// [1] : number? (AstTerminal)
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        /**************************************************/
        public override SdtsNode Build(SdtsParams param)
        {
            var identNode = Items[0].Build(param) as TerminalNode;
            NameToken = identNode.Token;

            // if array
            if(Items.Count > 1)
            {
                var literalNode = Items[1].Build(param) as TerminalNode;
                DimensionToken = literalNode.Token;
            }

            return this;
        }
    }
}
