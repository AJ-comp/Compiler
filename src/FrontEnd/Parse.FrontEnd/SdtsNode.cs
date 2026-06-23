using AJ.Common;
using Parse.Extensions;
using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Parse.FrontEnd
{
    public abstract class SdtsNode : IData, IHasParent, ITree<SdtsNode>
    {
        // for interface *********************************/
        public int Id { get; set; } = Interlocked.Decrement(ref _nextId);
        public int ParentId { get; set; }
        public string ParentType { get; set; }
        public int ChildIndex { get; set; }
        /********************************************/


        public string NodeName => GetType().Name;
        public bool IsNeedWhileIRGeneration { get; protected set; } = false;
        public AstSymbol Ast { get; protected set; }
        public SdtsNode Parent { get; set; }
        public List<SdtsNode> Items { get; } = new List<SdtsNode>();
        public MeaningErrInfoList ConnectedErrInfoList { get; } = new MeaningErrInfoList();
        public TokenDataList MeaningTokens => new TokenDataList(Ast?.AllTokens);
        public TokenDataList AllTokens => new TokenDataList(Ast?.ConnectedParseTree.AllTokens);

        private static int _nextId = int.MaxValue;

        public List<MeaningErrInfo> Alarms { get; } = new List<MeaningErrInfo>();


        public SdtsNode GetParent(Type toFindParent) => TreeHelper.GetParent(this, toFindParent);
        public SdtsNode GetParentAs(Type toFindParent) => TreeHelper.GetParentAs(this, toFindParent);


        public IEnumerable<SdtsNode> AllAlarmNodes
        {
            get
            {
                List<SdtsNode> result = new List<SdtsNode>();

                foreach (var item in Items)
                {
                    if (item.Alarms.Count > 0) result.Add(item);
                    result.AddRange(item.AllAlarmNodes);
                }

                return result.OrderBy(i => i.Id);
            }
        }

        public IEnumerable<SdtsNode> GetMatchedTypeNodes(Type type)
        {
            List<SdtsNode> result = new List<SdtsNode>();

            foreach (var child in Items)
            {
                if (child.GetType() == type) result.Add(child);

                result.AddRange(child.GetMatchedTypeNodes(type));
            }

            return result;
        }

        public abstract SdtsNode Compile(CompileParameter param);
        protected abstract SdtsNode CompileLogic(CompileParameter param);

        public override string ToString()
        {
            string result = $"Ast: {Ast}, Error node count: {AllAlarmNodes.Count()}";

            if (Items.Count > 0)
            {
                result += $", Expression: {NodeName} -> ";
                result += Items.ItemsString(PrintType.Type);
            }

            return result;
        }
    }
}
