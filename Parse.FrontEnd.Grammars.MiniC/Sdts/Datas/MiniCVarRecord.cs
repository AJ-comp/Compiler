using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.Datas
{
    public class MiniCVarRecord
    {
        public MiniCVarRecord(VariableMiniC varField, ReferenceInfo referenceInfo)
        {
            VarField = varField;
            ReferenceTable.Add(referenceInfo);
        }

        public VariableMiniC VarField { get; private set; }
        public List<ReferenceInfo> ReferenceTable { get; } = new List<ReferenceInfo>();
        public ExprNode InitValue => ReferenceTable[0].Value;

        public ReferenceInfo FindReferenceNode(Type type)
        {
            ReferenceInfo result = null;

            foreach (var record in ReferenceTable)
            {
                if (record.ReferenceNode.GetType() != type) continue;

                result = record;
            }

            return result;
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
