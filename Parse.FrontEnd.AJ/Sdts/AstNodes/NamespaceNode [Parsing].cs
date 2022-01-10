using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.FrontEnd.Ast;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public partial class NamespaceNode : AJNode
    {
        public NamespaceNode(AstSymbol node) : base(node)
        {
            BlockLevel = 1;
        }


        /**************************************************/
        /// <summary>
        /// Start semantic analysis for namespace.              <br/>
        /// namespace에 대한 의미분석을 시작합니다.        <br/>
        /// [0] : Ident (TerminalNode)
        /// [1:n] : (ClassDefNode | StructDefNode | EnumNode)*  (NonTerminal)
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        /**************************************************/
        public override SdtsNode Compile(CompileParameter param)
        {
            ConnectedErrInfoList.Clear();

            // parsing for ident
            var nameNode = Items[0].Compile(param) as TerminalNode;
            NameTokens.Add(nameNode.Token);

            // it needs to clone an param
            var newParam = param.Clone();

            var items = Items.Skip(1);
            foreach (var item in items)
            {
                var minicNode = item as AJNode;

                if (minicNode is ClassDefNode)
                {
                    newParam.Offset = 0;

                    var node = minicNode.Compile(newParam) as ClassDefNode;
                    if (!IsDuplicated(node.Token)) Classes.Add(node);
                }
                else if (minicNode is StructDefNode)
                {
                    newParam.Offset = 0;

                    var node = minicNode.Compile(newParam) as StructDefNode;
                    _structDefNodes.Add(node);
                }
            }

            References.Add(this);
            DBContext.Instance.Insert(this);

            return this;
        }

        private HashSet<StructDefNode> _structDefNodes = new HashSet<StructDefNode>();


        /****************************************************************/
        /// <summary>
        /// It checks if there is duplicated symbol name (struct, class, enum, etc) on namespace.
        /// 네임스페이스 내에 중복된 심벌 명 (struct, class, enum 등)이 있는지 검사합니다.
        /// </summary>
        /// <param name="toAddNameToken"></param>
        /// <returns></returns>
        /****************************************************************/
        private bool IsDuplicated(TokenData toAddNameToken)
        {
            // The parent node of NamespaceNode is ProgramNode.
            var parent = Parent as ProgramNode;

            // check if there is a same namespace in current file
            foreach (var namespaceData in parent.Namespaces)
            {
                if (namespaceData.FullName != FullName) continue;

                AddDuplicatedErrorInNamespace(toAddNameToken);
                return true;

                /*
                // in same namespace
                foreach (var symbol in namespaceData.AllSymbols)
                {
                    if (symbol.Name == toAddNameToken.Input)
                    {
                        AddDuplicatedErrorInNamespace(toAddNameToken);
                        return true;
                    }
                }
                */
            }

            return false;
        }


        /**************************************************/
        /// <summary>
        /// This function adds duplicated error to the node.
        /// 노드에 중복 심벌 오류를 추가합니다.
        /// </summary>
        /// <param name="varTokenToCheck"></param>
        /// <returns></returns>
        /**************************************************/
        private bool AddDuplicatedErrorInNamespace(TokenData token)
        {
            Alarms.Add(new MeaningErrInfo(token,
                                                nameof(AlarmCodes.MCL0021),
                                                string.Format(AlarmCodes.MCL0021, FullName, token.Input)));

            return true;
        }
    }
}
