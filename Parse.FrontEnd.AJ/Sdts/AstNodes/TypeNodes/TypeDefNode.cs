﻿using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Expressions;
using Parse.Types;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes
{
    public abstract class TypeDefNode : DefinitionNode
    {
        public Access AccessType { get; set; } = Access.Private;
        public TokenData NameToken { get; protected set; }
        public TokenDataList FullNameToken { get; } = new TokenDataList();
        public int Block { get; set; }
        public bool HeaderCompiled { get; protected set; } = false;

        // abstract
        public abstract AJDataType DefType { get; }
        public abstract uint Size { get; }
        public abstract IEnumerable<VariableAJ> AllFields { get; }
        public virtual IEnumerable<FuncDefNode> AllFuncs
        {
            get
            {
                List<FuncDefNode> result = new List<FuncDefNode>();

                foreach (var item in Items)
                {
                    if (!(item is FuncDefNode)) continue;

                    var funcDefNode = item as FuncDefNode;
                    if (funcDefNode.NameToken == null) continue;    // skip if not parsing yet

                    result.Add(funcDefNode);
                }

                result.AddRange(AutoGeneratedFuncs);

                return result;
            }
        }

        // readonly
        public string Name => NameToken?.Input;
        public string Namespace => RootNode.Namespace.FullName;
        public virtual string FullName => $"{Namespace}.{NameToken.Input}";

        public List<FuncDefNode> AutoGeneratedFuncs { get; } = new List<FuncDefNode>();

        public IEnumerable<TokenData> FullNameTokens
        {
            get
            {
                List<TokenData> result = new List<TokenData>();

                if (Parent is TypeDefNode)
                {
                    var parent = Parent as TypeDefNode;
                    result.AddRange(parent.FullNameTokens);
                }

                // namespace node is child node of the program node
                result.AddRange(RootNode.Namespace.NameTokens);

                result.Add(NameToken);

                return result;
            }
        }


        public string PopularName
        {
            get
            {
                string result = string.Empty;

                foreach (var predefType in PreDefTypeList)
                {
                    if (predefType.DefineFullName != FullName) continue;

                    result = predefType.ShortName;
                    break;
                }

                return result;
            }
        }


        public bool IsPreDefType
        {
            get
            {
                foreach (var predefType in PreDefTypeList)
                {
                    if (predefType.DefineFullName == FullName) return true;
                }

                return false;
            }
        }


        public TypeDefNode(AstSymbol node) : base(node)
        {

        }


        public IRStructDef ToIR()
        {
            IRStructDef result = new IRStructDef();

            result.Name = FullName;
            foreach (var field in AllFields.Where(s => s.Type.Const == false)) result.Members.Add(field.ToIR());

            return result;
        }

        public IRVariable ToIRVariable()
        {
            var type = new IRType(StdType.Struct, 1);
            type.Name = FullName;

            IRVariable result = new IRVariable(type, AJGrammar.This.Value, null, 0, 0, DebuggingData.CreateDummy());

            return result;
        }


        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            base.CompileLogic(param);

            if (param.Option == CompileOption.CheckAmbiguous)
            {
                if (SymbolTable.Instance.GetAllSameTypeDefNode(this, true).Count() > 0)
                {
                    AddAmbiguousError();
                }
            }

            return this;
        }


        /****************************************************************/
        /// <summary>
        /// <para>Check if there is duplicated symbol name (field, func, property) in class.</para>
        /// <para>클래스 내에 중복된 심벌 명 (field, func, property 등)이 있는지 검사합니다.</para>
        /// </summary>
        /// <param name="tokenToAdd"></param>
        /// <returns></returns>
        /****************************************************************/
        protected bool IsDuplicated(TokenData tokenToAdd)
        {
            foreach (var field in _fields)
            {
                if (field.Name != tokenToAdd.Input) continue;

                AddDuplicatedErrorInType(tokenToAdd);
                return true;
            }

            return false;
        }

        protected bool IsSameName(TokenData tokenToAdd)
        {
            if (tokenToAdd.Input != Name) return false;

            AddSameNameError(tokenToAdd);
            return true;
        }


        /// <summary>
        /// This function adds duplicated error to the node.
        /// </summary>
        /// <param name="duplicatedToken"></param>
        /// <returns></returns>
        public bool AddDuplicatedErrorInType(TokenData duplicatedToken)
        {
            Alarms.Add
            (
                new MeaningErrInfo(duplicatedToken,
                                                nameof(AlarmCodes.AJ0026),
                                                string.Format(AlarmCodes.AJ0026, Name, duplicatedToken))
            );

            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sameToken"></param>
        public void AddSameNameError(TokenData sameToken)
        {
            Alarms.Add(new MeaningErrInfo(sameToken, nameof(AlarmCodes.AJ0027), AlarmCodes.AJ0027));
        }


        public void AddAmbiguousError()
        {
            Alarms.Add(new MeaningErrInfo(NameToken,
                                                            nameof(AlarmCodes.AJ0040),
                                                            string.Format(AlarmCodes.AJ0040, FullName)));
        }



        protected List<VariableAJ> _fields = new List<VariableAJ>();


        /**************************************************/
        /// <summary>
        /// <para>Start semantic analysis for member variable.</para>
        /// <para>멤버 변수에 대한 의미분석을 시작합니다.</para>
        /// </summary>
        /// <param name="param"></param>
        /// <param name="offset"></param>
        /// <param name="accessType"></param>
        /**************************************************/
        protected void CompileForVariableDclsNode(CompileParameter param, int offset, Access accessType)
        {
            param.Offset = _fields == null ? 0 : _fields.Count();
            // children node is parsing only variable elements so it doesn't need to clone an param
            var varDclsNode = Items[offset].Compile(param) as DeclareVarStNode;

            foreach (var varData in varDclsNode.VarList)
            {
                if (IsSameName(varData.NameToken)) continue;
                if (IsDuplicated(varData.NameToken)) continue;

                varData.AccessType = accessType;
                _fields.Add(varData);
            }
        }


        /**************************************************/
        /// <summary>
        /// <para>Start semantic analysis for member function.</para>
        /// <para>멤버 함수에 대한 의미분석을 시작합니다.</para>
        /// </summary>
        /// <param name="param"></param>
        /// <param name="offset"></param>
        /// <param name="accessType"></param>
        /**************************************************/
        protected void CompileForFuncDefNode(CompileParameter param, int offset, Access accessType)
        {
            param.Offset = AllFuncs == null ? 0 : AllFuncs.Count();

            var node = Items[offset].Compile(param) as FuncDefNode;

            node.AccessType = accessType;
            if (IsSameName(node.NameToken)) return;
            if (IsDuplicated(node.NameToken)) return;
        }


        protected void CreateDefaultCreator(CompileParameter param)
        {
            // if defination of predef type then don't create constructor.
            // (ex: The 'defination' of bool (Boolean) have not to create constructor)
            if (IsPreDefType) return;

            param.Offset = AllFuncs == null ? 0 : AllFuncs.Count();
            AJType returnTypeInfo = new AJPreDefType(AJDataType.Void, null);

            var func = new FuncDefNode(FuncType.Creator, Access.Public, param.BlockLevel, param.Offset, this, returnTypeInfo);
            func.NameToken = TokenData.CreateStubToken(AJGrammar.Ident, Name);
            func.CompoundSt = new CompoundStNode(param.BlockLevel, param.Offset, this);

            AutoGeneratedFuncs.Add(func);
        }


        protected void CreateDefaultDestructor(CompileParameter param)
        {
            // if defination of predef type then don't create destructor.
            // (ex: The 'defination' of bool (Boolean) have not to create destructor)
            if (IsPreDefType) return;

            param.Offset = AllFuncs == null ? 0 : AllFuncs.Count();
            AJType returnTypeInfo = new AJPreDefType(AJDataType.Void, null);

            var func = new FuncDefNode(FuncType.Destructor, Access.Public, param.BlockLevel, param.Offset, this, returnTypeInfo);
            func.NameToken = TokenData.CreateStubToken(AJGrammar.Ident, $"~{Name}");
            func.CompoundSt = new CompoundStNode(param.BlockLevel, param.Offset, this);

            AutoGeneratedFuncs.Add(func);
        }
    }
}
