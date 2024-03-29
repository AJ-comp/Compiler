﻿using AJ.Common;
using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.StmtExpressions;
using Parse.Types;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public partial class FuncDefNode : DefinitionNode, ISymbolCenter, IExportable<IRExpression>
    {
        public CompoundStNode CompoundSt { get; internal set; }

        public IEnumerable<VariableAJ> VarList => ParamVarList;
        public IEnumerable<ISymbolData> SymbolList => VarList;

        public IRFunction IRFunction { get; } = new IRFunction();

        /// <summary>
        /// returns a host struct or class that own function.
        /// </summary>
        public TypeDefNode HostStruct
        {
            get
            {
                TypeDefNode result = null;
                var curNode = Parent;

                while (!(curNode is ProgramNode))
                {
                    if (curNode is TypeDefNode)
                    {
                        result = curNode as TypeDefNode;
                        break;
                    }

                    curNode = curNode.Parent;
                }

                return result;
            }
        }


        public FuncDefNode(AstSymbol node, FuncType type) : base(node)
        {
            FunctionalType = type;
            IsNeedWhileIRGeneration = true;
        }


        /// <summary>
        /// Creator to create a creator of class or struct.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="accessType"></param>
        /// <param name="blockLevel"></param>
        /// <param name="offset"></param>
        /// <param name="parentNode"></param>
        /// <param name="returnTypeInfo"></param>
        public FuncDefNode(FuncType type, Access accessType, int blockLevel, int offset, AJNode parentNode, AJType returnTypeInfo) : base(null)
        {
            FunctionalType = type;
            AccessType = accessType;
            Parent = parentNode;
            StubCode = true;

            var compileParam = new CompileParameter
            {
                BlockLevel = blockLevel,
                Offset = offset,
                RootNode = parentNode.RootNode,
            };

            base.CompileLogic(compileParam);
            ReturnType = returnTypeInfo;
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
            // it has to pass 'this' pointer as argument if it is not static function.
            if (HostStruct != null && !IsStatic)
            {
                IRFunction.Arguments.Add(HostStruct.ToIRVariable());
            }

            foreach (var arg in VarList)
                IRFunction.Arguments.Add(arg.ToIR());

            IRFunction.ReturnType = ReturnType.ToIR();
            IRFunction.Name = FullName;
            IRFunction.Statement = CompoundSt.To() as IRCompoundStatement;

            return IRFunction;
        }

        public IRExpression LazyTo()
        {
            return IRFunction;
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
            // if static is there is no 'this' pointer so offset is started to 0 but it is not static offset has to start to 1 (the offset of this pointer is 0)
            var newParam = (IsStatic) ? param.CloneForNewBlock(0) : param.CloneForNewBlock(1);
            var formalParam = Items[offset++].Compile(newParam) as ParamListNode;
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
