using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.Datas
{
    public class MiniCReferenceRecord<T>
    {
        public MiniCReferenceRecord(T defineField, ReferenceInfo referenceInfo)
        {
            DefineField = defineField;
            ReferenceList.Add(referenceInfo);
        }

        public T DefineField { get; private set; }
        public List<ReferenceInfo> ReferenceList { get; } = new List<ReferenceInfo>();
        public ExprNode InitValue => ReferenceList[0].Value;


        public ReferenceInfo FindReferenceNode(Type type)
        {
            ReferenceInfo result = null;

            foreach (var record in ReferenceList)
            {
                if (record.ReferenceNode.GetType() != type) continue;

                result = record;
            }

            return result;
        }

        public override bool Equals(object obj)
        {
            return obj is MiniCReferenceRecord<T> record &&
                   EqualityComparer<T>.Default.Equals(DefineField, record.DefineField);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DefineField);
        }
    }

    public class ReferenceInfo
    {
        public ReferenceInfo(MiniCNode referenceNode)
        {
            ReferenceNode = referenceNode;
        }

        public ReferenceInfo(MiniCNode referenceNode, ExprNode value)
        {
            ReferenceNode = referenceNode;
            Value = value;
        }

        public MiniCNode ReferenceNode { get; private set; }
        public ExprNode Value { get; private set; }
    }
}
