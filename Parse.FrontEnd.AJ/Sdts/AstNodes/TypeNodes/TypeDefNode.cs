using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes
{
    public abstract class TypeDefNode : AJNode
    {
        public abstract AJDataType Type { get; }
        public TokenData DataTypeToken { get; protected set; }
        public abstract uint Size { get; }
        public abstract string Name { get; }
        public abstract string FullName { get; }

        public TypeDefNode(AstSymbol node) : base(node)
        {

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
    }
}
