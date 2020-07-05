using Parse.MiddleEnd.IR.Datas;

namespace Parse.MiddleEnd.IR.LLVM.Models
{
    public class SSNode
    {
        /// <summary>
        /// Static Single Format
        /// </summary>
        public LocalVar SSF { get; }
        public IRData LinkedObject { get; }

        //public object LinkedValue
        //{
        //    get
        //    {
        //        var linkedObject = LinkedObject as TerminalItem;
        //        var result = (linkedObject is NamelessItem) ? (linkedObject as NamelessItem).Value : null;

        //        return result;
        //    }
        //}

        public SSNode(LocalVar current, IRData linkedObject)
        {
            SSF = current;
            LinkedObject = linkedObject;
        }
    }
}
