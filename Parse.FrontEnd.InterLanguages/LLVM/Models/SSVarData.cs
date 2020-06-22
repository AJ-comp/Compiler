using Parse.FrontEnd.InterLanguages.Datas;

namespace Parse.FrontEnd.InterLanguages.LLVM.Models
{
    public abstract class SSVarData : IRData
    {
        public abstract int Offset { get; }
        public abstract object LinkedObject { get; internal set; }

        public string Name
        {
            get
            {
                string result = "%" + Offset;

                if (LinkedObject is IRVarData)
                {
                    var linkedData = LinkedObject as IRVarData;
                    if (linkedData.IsGlobal) result = "@" + linkedData.Name;
                }

                return result;
            }
        }

        public bool IsLinkedObject<T>(T data) where T : class
        {
            if ((LinkedObject is T) == false) return false;

            T linkedData = LinkedObject as T;
            if (linkedData.Equals(data) == false) return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is SSVarData data &&
                   Offset == data.Offset;
        }

        public override int GetHashCode()
        {
            return -149965190 + Offset.GetHashCode();
        }
    }
}
