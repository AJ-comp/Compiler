using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public class ProgramNode : AJNode
    {
        public List<UsingStNode> UsingNamespaces { get; } = new List<UsingStNode>();
        public NamespaceNode Namespace { get; private set; }


        public HashSet<IDeclareable> ShortCutDeclareVarSet { get; } = new HashSet<IDeclareable>();
        public HashSet<AJNode> UnLinkedSymbols { get; } = new HashSet<AJNode>();
        public HashSet<AJNode> LinkedSymbols { get; } = new HashSet<AJNode>();
        public HashSet<AJNode> AmbiguityLinkedSymbols { get; } = new HashSet<AJNode>();
        public HashSet<AJNode> CompletedSymbols { get; } = new HashSet<AJNode>();


        public bool IsBuild { get; private set; }
        public string FullPath { get; set; }

        public List<Exception> FiredExceptoins { get; } = new List<Exception>();


        public IEnumerable<ProgramNode> AccessablePrograms
        {
            get
            {
                HashSet<ProgramNode> result = new HashSet<ProgramNode>();

                foreach (var usingNamespace in UsingNamespaces)
                {
                    if (Datas.SymbolTable.Instance.ContainsKey(usingNamespace.FullName))
                        result.UnionWith(SymbolTable.Instance[usingNamespace.FullName]);
                }

                result.UnionWith(SymbolTable.Instance[Namespace.FullName]);

                return result;
            }
        }


        public IEnumerable<TypeDefNode> DefTypes
        {
            get
            {
                List<TypeDefNode> result = new List<TypeDefNode>();

                foreach (var item in Items)
                {
                    if (!(item is TypeDefNode)) continue;

                    var typeNode = item as TypeDefNode;
                    // The NameToken is null means not parsed yet.
                    if (typeNode.NameToken == null) continue;

                    result.Add(typeNode);
                }

                return result;
            }
        }


        public ProgramNode(AstSymbol node) : base(node)
        {
            IsNeedWhileIRGeneration = true;
        }

        // [0:n] : Using? (AstNonTerminal)
        // [n+1:1] : Namespace (AstNonTerminal)
        // [n+2:y] : (ClassDefNode | StructDefNode | EnumNode)*  (NonTerminal)   <br/>
        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            param.RootNode = this;

            base.CompileLogic(param);
            IsBuild = param.Build;
            FullPath = param.FileFullPath;

            param.RootNode = this;
            if (param.Option == CompileOption.CheckUsing) UsingCompile(param);
            else if (param.Option == CompileOption.CheckNamespace) NamespaceCompile(param);
            else if (param.Option == CompileOption.CheckTypeDefine) DefineCompile(param);
            else if (param.Option == CompileOption.CheckMemberDeclaration) DefineCompile(param);
            else if (param.Option == CompileOption.CheckAmbiguous) DefineTypeAmbiguousCompile(param);
            else if (param.Option == CompileOption.Logic) DefineCompile(param);

            return this;
        }


        public override string ToString() => $"{FileFullPath} {GetType().Name}";


        private void UsingCompile(CompileParameter param)
        {
            foreach (var item in Items)
            {
                var ajNode = item as AJNode;

                // using statement list
                if (ajNode is UsingStNode)
                {
                    UsingNamespaces.Add(ajNode.Compile(param) as UsingStNode);
                }
            }
        }


        private void NamespaceCompile(CompileParameter param)
        {
            foreach (var item in Items)
            {
                // namespace statement list
                if (item is NamespaceNode)
                {
                    Namespace = item.Compile(param) as NamespaceNode;
                    break;
                }
            }

            SymbolTable.Instance.Add(this);
        }


        private void DefineCompile(CompileParameter param)
        {
            int newBlockOffset = 0;
            foreach (var item in Items)
            {
                if (item is ClassDefNode)
                {
                    var node = item.Compile(param.CloneForNewBlock(newBlockOffset++)) as ClassDefNode;
                }
                else if (item is StructDefNode)
                {
                    var node = item.Compile(param.CloneForNewBlock(newBlockOffset++)) as StructDefNode;
                }
            }
        }


        private void UnLinkCompile(CompileParameter param)
        {
            // retry compile for undefined symbol
            var copiedUnLinkedSymbols = new HashSet<SdtsNode>(AllAlarmNodes);
            foreach (var unlinkNode in copiedUnLinkedSymbols)
            {
                if (unlinkNode is ExprNode)
                {
                    var exprNode = unlinkNode as ExprNode;
                    if (!exprNode.IsRoot) continue;

                    exprNode.Compile(null);
                }
                else if (unlinkNode is DeclareVarNode)
                {
                    unlinkNode.Compile(null);
                }
            }
        }


        private void DefineTypeAmbiguousCompile(CompileParameter param)
        {
            foreach (var item in Items)
            {
                if (!(item is TypeDefNode)) continue;

                var typeNode = item as TypeDefNode;
                if(SymbolTable.Instance.GetAllSameTypeDefNode(typeNode).Count() > 1)
                    typeNode.AddAmbiguousError();

            }
        }
    }
}
