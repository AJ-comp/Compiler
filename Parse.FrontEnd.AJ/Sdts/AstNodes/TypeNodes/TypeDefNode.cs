using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.FrontEnd.Ast;
using System.Linq;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes
{
    public abstract class TypeDefNode : AJNode, ISymbolData
    {
        public TokenData NameToken { get; protected set; }
        public TokenDataList FullNameToken { get; } = new TokenDataList();
        public int Block { get; set; }
        public bool HeaderCompiled { get; protected set; } = false;

        // abstract
        public abstract AJDataType Type { get; }
        public abstract uint Size { get; }

        // readonly
        public string Name => NameToken?.Input;
        public string Namespace => RootNode.Namespace.FullName;
        public virtual string FullName => $"{Namespace}.{NameToken.Input}";


        public TypeDefNode(AstSymbol node) : base(node)
        {

        }


        public override SdtsNode Compile(CompileParameter param)
        {
            base.Compile(param);

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
        protected abstract bool IsDuplicated(TokenData tokenToAdd);
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
    }
}
