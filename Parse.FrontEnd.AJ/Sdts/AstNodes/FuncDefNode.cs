using AJ.Common;
using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.StmtExpressions;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public partial class FuncDefNode : DefinitionNode, ISymbolCenter, IExportable<IRExpression>
    {
        public CompoundStNode CompoundSt { get; private set; }

        public IEnumerable<VariableAJ> VarList => ParamVarList;
        public IEnumerable<ISymbolData> SymbolList => VarList;


        public FuncDefNode(AstSymbol node, FuncType type) : base(node)
        {
            FunctionalType = type;
            IsNeedWhileIRGeneration = true;
        }

        public FuncDefNode(FuncType type, Access accessType, int blockLevel, int offset, SdtsNode rootNode, AJType returnTypeInfo) : base(null)
        {
            FunctionalType = type;
            AccessType = accessType;
            var compileParam = new CompileParameter
            {
                BlockLevel = blockLevel,
                Offset = offset,
                RootNode = rootNode
            };

            base.CompileLogic(compileParam);
            ReturnType = returnTypeInfo;
            StubCode = true;
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
        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            // it needs to clone an param
            base.CompileLogic(param);

            if (param.Option == CompileOption.CheckMemberDeclaration) DefineCompile(param);
            else if (param.Option == CompileOption.Logic) DetailCompile(param);

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


        private void DefineCompile(CompileParameter param)
        {
            var classDefNode = GetParent(typeof(ClassDefNode)) as ClassDefNode;

            int offset = 0;
            if (Items[offset] is TerminalNode)
            {
                offset++;
                ReturnType.Const = true;
            }

            if (FunctionalType == FuncType.Normal)
            {
                // compile return type
                var declareIdent = Items[offset++].Compile(param.CloneForNewBlock()) as TypeDeclareNode;
                ReturnType = declareIdent.ToAJTypeInfo(true);
            }
            else ReturnType = new AJUserDefType(classDefNode);

            // compile function name
            var nameNode = Items[offset++].Compile(param.CloneForNewBlock()) as DefNameNode;
            NameToken = nameNode.Token;

            CheckIsDuplicatedFunction();

            // add this reference
            //            ParamVarList.Add(VariableAJ.CreateThisVar(classDefNode.Type, classDefNode.NameToken, BlockLevel + 1, 0));
            var formalParam = Items[offset++].Compile(param.CloneForNewBlock(1)) as ParamListNode;
            ParamVarList.AddRange(formalParam.VarList);

            Reference.Add(this);

            stmtNodeIndex = offset;
        }

        private void DetailCompile(CompileParameter param)
        {
            CompoundSt = Items[stmtNodeIndex++].Compile(param.CloneForNewBlock()) as CompoundStNode;

            if (ReturnType.DataType != AJDataType.Void)
            {
                if (!CompoundSt.ClarifyReturn) AddNotClarifyReturnError();
            }
        }

        private void AddNotClarifyReturnError()
        {
            Alarms.Add(new MeaningErrInfo(NameToken, nameof(AlarmCodes.AJ0029), AlarmCodes.AJ0029));
        }

        /// <summary>
        /// Checks if there is a duplicated function in class.    <br/>
        /// Checks using function name and parameter.       <br/>
        /// </summary>
        /// <param name="func"></param>
        /// <returns>return true if there is no, return false if there is a duplicated function</returns>
        public void CheckIsDuplicatedFunction()
        {
            if (NameToken.IsVirtual) return;

            var classDefNode = GetParent(typeof(ClassDefNode)) as ClassDefNode;

            foreach (var func in classDefNode.AllFuncs.Where(x => x.StubCode == false && x != this))
            {
                if (func.IsEqualFunction(this))
                {
                    Alarms.Add(AJAlarmFactory.CreateAJ0026(classDefNode, NameToken));
                    break;
                }
            }
        }

        private int stmtNodeIndex = -1;
    }
}
