namespace Parse.MiddleEnd.IR.LLVM.Models
{
    public class SSNode
    {
        /// <summary>
        /// Static Single Format
        /// </summary>
        public LocalVar SSF { get; }
        public SSItem LinkedObject { get; }

        public bool IsLinkedTerminalItem => (LinkedObject is TerminalItem);
        public object LinkedValue
        {
            get
            {
                if (!IsLinkedTerminalItem) return null;

                var linkedObject = LinkedObject as TerminalItem;
                var result = (linkedObject is NamelessItem) ? (linkedObject as NamelessItem).Value : null;

                return result;
            }
        }

        public SSNode(LocalVar current, SSItem linkedObject)
        {
            SSF = current;
            LinkedObject = linkedObject;
        }
    }
}
