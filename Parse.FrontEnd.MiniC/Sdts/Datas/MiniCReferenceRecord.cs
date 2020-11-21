using Parse.FrontEnd.MiniC.Sdts.AstNodes;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Parse.FrontEnd.MiniC.Sdts.Datas
{
    [DebuggerDisplay("{DebuggerDisplay, nq}")]
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



        private string DebuggerDisplay
        {
            get
            {
                var result = string.Format("DefineField: {0}, Reference count: {1}",
                                                        DefineField.GetType().Name,
                                                        ReferenceList.Count);

                if (InitValue?.Result != null)
                {
                    result += string.Format("Init value: {0},{1},{2}",
                                                        Helper.GetDescription(InitValue.Result.TypeName),
                                                        InitValue.Result.Value,
                                                        InitValue.Result.ValueState);
                }

                return result;
            }
        }
    }

    [DebuggerDisplay("{DebuggerDisplay, nq}")]
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


        private string DebuggerDisplay
            => string.Format("Reference node: {0}, Value: {1}", ReferenceNode.GetType(), Value?.Result.ToString());
    }
}
