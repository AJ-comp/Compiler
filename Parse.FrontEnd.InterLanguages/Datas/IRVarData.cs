using System.Collections.Generic;

namespace Parse.FrontEnd.InterLanguages.Datas
{
    public class IRVarData : IRData
    {
        public DataType Type { get; }
        public string Name { get; }
        public int Block { get; }
        public int Offset { get; }

        public int Length { get; }
        public bool IsGlobal => (Block == 0) ? true : false;

        public IRVarData(DataType type, string name, int block, int offset, int length)
        {
            Type = type;
            Name = name;
            Block = block;
            Offset = offset;
            Length = length;
        }

        public override bool Equals(object obj)
        {
            return obj is IRVarData data &&
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
