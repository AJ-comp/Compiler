using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.Ast;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public class DeclareVarNode : DeclareIdentNode, IDeclareable
    {
        public VariableAJ Variable { get; private set; }

        public string VarName => NameToken?.Input;
        public IEnumerable<int> ArrayDimension
        {
            get
            {
                List<int> result = new List<int>();

                foreach (var item in ArrayTokens) result.Add(System.Convert.ToInt32(item.Input));

                return result;
            }
        }

        public IEnumerable<TokenData> ArrayTokens => _arrayTokens;
        public IEnumerable<VariableAJ> VarList => new List<VariableAJ>() { Variable };

        public DeclareVarNode(AstSymbol node) : base(node)
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
        /// [3] : expression? (AstNonTerminal)  <br/>
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        /**************************************************/
        public override SdtsNode Compile(CompileParameter param)
        {
            base.Compile(param);

            Variable = new VariableAJ(Access.Private, AJType, NameToken, null, BlockLevel, Offset);
            Variable.CreatedNode = this;

            if (Items.Last() is ExprNode)
            {
                _initExpression = Items.Last().Compile(param) as ExprNode;
                Variable.InitValue = _initExpression;
            }
            else if (Variable.VariableType == VarType.ValueType)
            {
            }

            // if link-time then it must not check duplication.
            // In link-time, if the duplication check is performed the error will be fired. the symbol was already inserted at first compile time.
            if (param != null)
            {
                var symbol = GetSymbolFromCurrentBlock(Variable.NameToken);
                if (symbol != null) Alarms.Add(AJAlarmFactory.CreateMCL0009(Variable.NameToken));
            }

            return this;
        }


        private ExprNode _initExpression;
        private List<TokenData> _arrayTokens = new List<TokenData>();
    }
}
