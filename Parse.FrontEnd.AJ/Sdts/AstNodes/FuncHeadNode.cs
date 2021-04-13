using Parse.FrontEnd.Ast;
using System.Linq;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public class FuncHeadNode : AJNode
    {
        public string Name => NameToken?.Input;

        public VariableTypeNode ReturnType { get; private set; }
        public TokenData NameToken { get; private set; }
        public ParamListNode ParamVarList { get; private set; }

        public FuncHeadNode(AstSymbol node) : base(node)
        {
        }



        /**********************************************/
        /// <summary>
        /// It starts semantic analysis for function header.         <br/>
        /// 함수 헤더에 대한 의미분석을 시작합니다.               <br/>
        /// </summary>
        /// <remarks>
        /// [0] : VariableTypeNode [DclSpec]                            <br/>
        /// [1] : TerminalNode [Name]                                     <br/>
        /// [2] : FormalPara (AstNonTerminal)                          <br/>
        /// </remarks>
        /// <param name="param"></param>
        /// <returns></returns>
        /**********************************************/
        public override SdtsNode Build(SdtsParams param)
        {
            BlockLevel = ParentBlockLevel + 1;

            // build DclSpec node
            ReturnType = Items[0].Build(param) as VariableTypeNode;
            // set Name data
            NameToken = (Items[1].Build(param) as TerminalNode).Token;
            // build FormalPara node
            ParamVarList = Items[2].Build(param) as ParamListNode;

            var classDefNode = GetParent(typeof(ClassDefNode)) as ClassDefNode;
            var symbols = classDefNode.SymbolList
                                                    .AsParallel()
                                                    .Where(s => s.Name == NameToken.Input).ToList();
            if (symbols.Count() > 0)
            {
                AddDuplicatedError(NameToken);
            }

            return this;
        }
    }
}
