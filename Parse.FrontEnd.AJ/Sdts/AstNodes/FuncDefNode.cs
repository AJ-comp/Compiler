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

        public FuncDefNode(FuncType type, Access accessType, int blockLevel, int offset, AJTypeInfo returnTypeInfo) : base(null)
        {
            Type = type;
            AccessType = accessType;
            BlockLevel = blockLevel;
            Offset = offset;
            TypeData = returnTypeInfo;
        }


        /**************************************************/
        /// <summary>
        /// <para>Start semantic analysis for function defenition.</para>
        /// <para>함수 정의에 대한 의미분석을 시작합니다.</para>
        /// </summary>
        /// <remarks>
        /// [0] : declarator (DeclareVarNode)                <br/>
        /// [1] : FormalPara (ParamListNode)                <br/>
        /// [2] : CompoundSt (CompoundStNode)          <br/>
        /// </remarks>
        /// <param name="param"></param>
        /// <returns></returns>
        /**************************************************/
        public override SdtsNode Compile(CompileParameter param)
        {
            // it needs to clone an param
            var newParam = param.Clone();
            var classDefNode = GetParent(typeof(ClassDefNode)) as ClassDefNode;

            Type = FuncType.Normal;
            BlockLevel = ParentBlockLevel;
            AccessType = Access.Private;

            // build FuncHead node
            var declareIdent = Items[0].Compile(newParam) as DeclareIdentNode;

            TypeData = declareIdent.AJType;
            Token = declareIdent.NameToken;

            // add this reference
            ParamVarList.Add(VariableAJ.CreateThisVar(classDefNode.Type, BlockLevel, 0));
            var formalParam = Items[1].Compile(newParam) as ParamListNode;
            ParamVarList.AddRange(formalParam.VarList);

            Reference.Add(this);

            // build CompoundSt node
            CompoundSt = Items[1].Compile(newParam) as CompoundStNode;

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
