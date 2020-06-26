using System.Collections.Generic;

namespace Parse.FrontEnd.InterLanguages.Datas
{
    public abstract class IRVar : IRData
    {
        public DataType Type { get; }
        public string Name { get; }
        public int Block { get; }
        public int Offset { get; }

        public int Length { get; }
        public bool IsGlobal => (Block == 0);

        public abstract bool IsSigned { get; }
        public abstract bool IsNan { get; }

        public IRVar(DataType type, string name, int block, int offset, int length)
        {
            Type = type;
            Name = name;
            Block = block;
            Offset = offset;
            Length = length;
        }

        public override bool Equals(object obj)
        {
            return obj is IRVar data &&
                   Name == data.Name &&
                   Block == data.Block &&
                   Offset == data.Offset;
        }

        public override int GetHashCode()
        {
            int hashCode = -1126147934;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + Block.GetHashCode();
            hashCode = hashCode * -1521134295 + Offset.GetHashCode();
            return hashCode;
        }
    }
}
