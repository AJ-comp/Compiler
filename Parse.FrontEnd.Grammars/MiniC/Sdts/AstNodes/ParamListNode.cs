﻿using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;
using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes
{
    public class ParamListNode : MiniCNode
    {
        public IReadOnlyList<ParamNode> ParamNodes => _paramNodes;
        public IReadOnlyList<MiniCVarData> ToVarDataList
        {
            get
            {
                List<MiniCVarData> result = new List<MiniCVarData>();

                foreach (var node in ParamNodes) result.Add(node.ToVarData);

                return result;
            }
        }

        public ParamListNode(AstSymbol node) : base(node)
        {
        }



        // format summary [Can induce epsilon]
        // [0:n] : ParamNode [ParamDcl]
        public override SdtsNode Build(SdtsParams param)
        {
            foreach (var item in Items)
            {
                _paramNodes.Add(item.Build(param) as ParamNode);
                param.Offset++;
            }

            return this;
        }


        private List<ParamNode> _paramNodes = new List<ParamNode>();
    }
}
