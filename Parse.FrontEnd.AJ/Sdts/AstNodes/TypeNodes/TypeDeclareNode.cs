using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.Ast;
using System.Collections.Generic;
using System.Linq;

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
            }

            CheckDefineForType();

            return this;
        }


        public AJType ToAJTypeInfo(bool bConst)
        {
            if(TypeKind == TypeKind.PreDef)
            {
                AJType result = new AJPreDefType(Type, FullDataTypeToken)
                {
                    Const = bConst,
                    Signed = Signed
                };

                return result;
            }
            else
            {
                var defNodes = GetDefineForType(FullDataTypeToken.ToListString());
                return (defNodes.Count() == 1) ? new AJUserDefType(defNodes.First()) : new AJUserDefType(null);
            }
        }


        private void CheckDefineForType()
        {
            // the predef type exists the define certainly.
            if (TypeKind == TypeKind.PreDef) return;

            var typeSymbols = GetDefineForType(FullName);
            if (typeSymbols.Count() > 1)    // the type is ambiguity
                Alarms.Add(AJAlarmFactory.CreateAJ0039(this, typeSymbols.ElementAt(0), typeSymbols.ElementAt(1)));
            else if (typeSymbols.Count() == 0)   // there is no define for type
                Alarms.Add(AJAlarmFactory.CreateAJ0031(FullDataTypeToken));
            else DefNode = typeSymbols.First();
        }


        public override AJDataType Type { get; }
        public override uint Size => AJUtilities.SizeOf(this);

    }
}
