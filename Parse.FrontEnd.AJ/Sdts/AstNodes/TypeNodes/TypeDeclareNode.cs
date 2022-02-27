using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.Ast;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes
{
    public enum TypeKind
    {
        PreDef,
        UserDef
    }

    public class TypeDeclareNode : DataTypeNode
    {
        public bool Signed { get; } = false;
        public TypeKind TypeKind { get; }
        public TypeDefNode DefNode { get; private set; }

        // predef type
        public TypeDeclareNode(AstSymbol node, AJDataType stdType, bool signed, string name) : base(node)
        {
            Type = stdType;
            Signed = signed;

            TypeKind = TypeKind.PreDef;
        }

        // userdef type
        public TypeDeclareNode(AstSymbol node) : base(node)
        {
            Type = AJDataType.Unknown;

            TypeKind = TypeKind.UserDef;
        }


        /// <summary>
        /// predef type: [0]
        /// userdef type: [0:n]
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public override SdtsNode Compile(CompileParameter param)
        {
            base.Compile(param);

            if (TypeKind == TypeKind.PreDef)
            {
                var node = Items[0].Compile(param) as TerminalNode;

                // yet the predef type doesn't have to check if virtual token.
                FullDataTypeToken.Add(node.Token);
            }
            else if (TypeKind == TypeKind.UserDef)
            {
                int offset = 0;
                while (offset < Items.Count)
                {
                    var node = Items[offset++].Compile(param) as TerminalNode;
                    if (node.Token.IsVirtual) continue;

                    FullDataTypeToken.Add(node.Token);
                }

                foreach (var ns in RootNode.Namespaces)
                {
                    foreach (var cs in ns.DefTypes)
                    {
                        if (cs.FullName == FullName) DefNode = cs;
                        else if (cs.Name == Name) DefNode = cs;
                    }
                }
            }

            return this;
        }


        public AJTypeInfo ToAJTypeInfo(bool bConst)
        {
            AJTypeInfo result = new AJTypeInfo(Type, DataTypeToken)
            {
                Const = bConst,
                Signed = Signed
            };

            return result;
        }


        public override AJDataType Type { get; }
        public override uint Size => AJUtilities.SizeOf(this);

    }
}
