using AJ.Common;
using Parse.Extensions;
using Parse.MiddleEnd.IR.Datas;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Parse.MiddleEnd.IR.Expressions
{
    public abstract class IRExpression : ITree<IRExpression>
    {
        public int Id { get; } = Interlocked.Decrement(ref _nextId);
        public IRExpression Parent { get; set; }
        public List<IRExpression> Items { get; } = new List<IRExpression>();
        public bool AssignPosition { get; private set; } = false;

        public DebuggingData DebuggingData { get; } = DebuggingData.CreateDummy();
        public static bool ShowLineColumn { get; } = false;


        public void SetAssignPosition(bool value)
        {
            foreach (var item in Items)
                item.SetAssignPosition(value);
        }

        public IRExpression GetParent(Type toFindParent) => TreeHelper.GetParent(this, toFindParent);
        public IRExpression GetParentAs(params Type[] toFindParents) => TreeHelper.GetParentAs(this, toFindParents);

        public override string ToString()
        {
            if (!ShowLineColumn) return $"{DebuggingData.Source}";

            var sLine = DebuggingData.StartLine;
            var eLine = DebuggingData.EndLine;
            var sColumn = DebuggingData.StartColumn;
            var eColumn = DebuggingData.EndColumn;

            return DebuggingData.bMeanIndex ? $"{DebuggingData.Source} [line: {sLine}:{eLine}] [column: {sColumn}:{eColumn}]"
                                                                : $"{DebuggingData.Source}";
        }


        private static int _nextId = int.MaxValue;
        protected List<ITable> _tables = new List<ITable>();

        protected IRExpression()
        {
        }


        protected IRExpression(DebuggingData debuggingData)
        {
            DebuggingData = debuggingData;
        }
    }
}
