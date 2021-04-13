using Parse.FrontEnd.Ast;
using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts.Datas;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes
{
    public class NamespaceNode : AJNode, INamespaceExpression
    {
        public string Name => NamespaceData.Name;
        public IEnumerable<IClassExpression> ClassDatas => NamespaceData?.ClassDatas;
        public NamespaceData NamespaceData { get; private set; }

        public NamespaceNode(AstSymbol node) : base(node)
        {
            BlockLevel = 1;
        }


        /**************************************************/
        /// <summary>
        /// It starts semantic analysis for namespace.
        /// namespace에 대한 의미분석을 시작합니다.
        /// [0] : Ident (TerminalNode)
        /// [1:n] : (ClassDefNode | StructDefNode | EnumNode)*  (NonTerminal)
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        /**************************************************/
        public override SdtsNode Build(SdtsParams param)
        {
            ConnectedErrInfoList.Clear();

            HashSet<ClassDefData> _classDatas = new HashSet<ClassDefData>();

            // parsing for ident
            var nameNode = Items[0].Build(param) as TerminalNode;

            List<TokenData> identTokens = new List<TokenData>();
            identTokens.Add(nameNode.Token);
            NamespaceData = new NamespaceData(BlockLevel, identTokens, _classDatas);

            // it needs to clone an param
            var newParam = param.Clone() as AJSdtsParams;

            var items = Items.Skip(1);
            foreach (var item in items)
            {
                var minicNode = item as AJNode;

                if (minicNode is ClassDefNode)
                {
                    newParam.Offset = 0;

                    var node = minicNode.Build(newParam) as ClassDefNode;
                    if (!IsDuplicated(node.ClassData.NameToken)) _classDatas.Add(node.ClassData);
                }
                else if (minicNode is StructDefNode)
                {
                    newParam.Offset = 0;

                    var node = minicNode.Build(newParam) as StructDefNode;
                    _structDefNodes.Add(node);
                }
            }

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
            // check if there is duplicated symbol name on namespace of current file.
            foreach(var symbol in NamespaceData.AllSymbols)
            {
                if (symbol.Name == toAddNameToken.Input)
                {
                    AddDuplicatedErrorInNamespace(toAddNameToken);
                    return true;
                }
            }

            // The parent node of NamespaceNode is ProgramNode.
            var parent = Parent as ProgramNode;

            // It has to check same namespace in other file.
            foreach (var namespaceData in parent.NamespaceDatas)
            {
                if (namespaceData.Name != NamespaceData.Name) continue;

                // in same namespace
                foreach (var symbol in namespaceData.AllSymbols)
                {
                    if (symbol.Name == toAddNameToken.Input)
                    {
                        AddDuplicatedErrorInNamespace(toAddNameToken);
                        return true;
                    }
                }
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
            ConnectedErrInfoList.Add
            (
                new MeaningErrInfo(token,
                                                nameof(AlarmCodes.MCL0021),
                                                string.Format(AlarmCodes.MCL0021, NamespaceData.Name, token.Input))
            );

            return true;
        }
    }
}
