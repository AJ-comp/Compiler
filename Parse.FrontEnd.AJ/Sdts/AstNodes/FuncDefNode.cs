using AJ.Common;
using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.StmtExpressions;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public partial class FuncDefNode : AJNode, ISymbolCenter, IExportable<IRExpression>
    {
        public CompoundStNode CompoundSt { get; private set; }

        public IEnumerable<VariableAJ> VarList => ParamVarList;
        public IEnumerable<ISymbolData> SymbolList => VarList;


        public FuncDefNode(AstSymbol node) : base(node)
        {
            IsNeedWhileIRGeneration = true;
        }

        public FuncDefNode(FuncType type, Access accessType, int blockLevel, int offset, SdtsNode rootNode, AJTypeInfo returnTypeInfo) : base(null)
        {
            Type = type;
            AccessType = accessType;
            var compileParam = new CompileParameter();
            compileParam.BlockLevel = blockLevel;
            compileParam.Offset = offset;
            compileParam.RootNode = rootNode;

            base.Compile(compileParam);
            ReturnTypeData = returnTypeInfo;
        }


        /**************************************************/
        /// <summary>
        /// <para>Start semantic analysis for function defenition.</para>
        /// <para>함수 정의에 대한 의미분석을 시작합니다.</para>
        /// </summary>
        /// <remarks>
        /// [0] : const? (TerminalNode)                         <br/>
        /// [1] : type_spec (TypeDeclareNode)               <br/>
        /// [2] : CompoundSt (CompoundStNode)           <br/>
        /// </remarks>
        /// <param name="param"></param>
        /// <returns></returns>
        /**************************************************/
        public override SdtsNode Compile(CompileParameter param)
        {
            // it needs to clone an param
            base.Compile(param);
            var classDefNode = GetParent(typeof(ClassDefNode)) as ClassDefNode;

            int offset = 0;
            if (Items[offset] is TerminalNode)
            {
                offset++;
                ReturnTypeData.Const = true;
            }

            // compile return type
            var declareIdent = Items[offset++].Compile(param.CloneForNewBlock()) as TypeDeclareNode;
            ReturnTypeData = new AJTypeInfo(declareIdent.Type, declareIdent.DataTypeToken);

            // compile function name
            var nameNode = Items[offset++].Compile(param.CloneForNewBlock()) as DefNameNode;
            NameToken = nameNode.Token;

            // add this reference
            ParamVarList.Add(VariableAJ.CreateThisVar(classDefNode.Type, classDefNode.NameToken, BlockLevel + 1, 0));
            var formalParam = Items[offset++].Compile(param.CloneForNewBlock(1)) as ParamListNode;
            ParamVarList.AddRange(formalParam.VarList);

            Reference.Add(this);

            // compile CompoundSt node
            CompoundSt = Items[offset++].Compile(param.CloneForNewBlock()) as CompoundStNode;

            return this;
        }

        public IRExpression To()
        {
            IRFunction function = new IRFunction();

            foreach (var arg in VarList)
                function.Arguments.Add(arg.ToIR());

            function.Statement = CompoundSt.To() as IRCompoundStatement;

            return function;
        }

        public IRExpression To(IRExpression from)
        {
            throw new System.NotImplementedException();
        }
    }
}
